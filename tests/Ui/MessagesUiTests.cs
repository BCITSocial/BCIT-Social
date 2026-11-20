using BCIT_Social.Tests.Support;
using Microsoft.Playwright;

namespace BCIT_Social.Tests.Ui;

/// <summary>
/// Pure UI unit tests for wwwroot/js/messages.js running in headless Chromium via Playwright (U01–U06).
/// Exercises the composer, live inbox filter, debounced alumni search, race-condition protection,
/// form-submit guards, and in-conversation search against the Razor DOM contract.
/// </summary>
[Collection("PlaywrightUi")]
public class MessagesUiTests(PlaywrightFixture fx)
{
    // U01
    [Fact]
    public async Task Composer_EnterSends_ShiftEnterAndImeEnterDoNot_AndSendStaysDisabledWhileBlank()
    {
        var page = await fx.OpenComponentPageAsync("/Messages", UiDomFixtures.MessagesPage(), "messages.js");
        try
        {
            await page.EvaluateAsync("""
                () => {
                    window.__submitCount = 0;
                    document.querySelector('[data-message-form]').requestSubmit = () => { window.__submitCount++; };
                }
                """);

            var box = page.Locator("[data-message-form] textarea[name='text']");
            var send = page.Locator("[data-message-form] .msg-send");

            (await send.IsDisabledAsync()).ShouldBeTrue();

            await box.FillAsync("   ");
            (await send.IsDisabledAsync()).ShouldBeTrue();

            await box.FillAsync("hello");
            (await send.IsDisabledAsync()).ShouldBeFalse();

            await box.PressAsync("Enter");
            (await page.EvaluateAsync<int>("() => window.__submitCount")).ShouldBe(1);

            await box.PressAsync("Shift+Enter");
            await page.EvaluateAsync("""
                () => {
                    const input = document.querySelector("[data-message-form] textarea[name='text']");
                    input.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true, cancelable: true, isComposing: true }));
                }
                """);

            (await page.EvaluateAsync<int>("() => window.__submitCount")).ShouldBe(1);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U02
    [Fact]
    public async Task ConversationListSearch_FiltersCaseInsensitively_AnnouncesCount_AndRestoresOnClear()
    {
        var page = await fx.OpenComponentPageAsync("/Messages", UiDomFixtures.MessagesPage(), "messages.js");
        try
        {
            var search = page.Locator("[data-message-search]");
            var status = page.Locator("[data-message-search-status]");
            var empty = page.Locator("[data-message-search-empty]");
            var visibleRows = page.Locator(".msg-conversations > li:not([hidden])");

            await search.FillAsync("MAYA");
            (await visibleRows.CountAsync()).ShouldBe(1);
            (await status.TextContentAsync()).ShouldBe("1 conversation found");

            await search.FillAsync("noon");
            (await visibleRows.CountAsync()).ShouldBe(1);
            (await visibleRows.First.TextContentAsync())!.ShouldContain("Priya S.");

            await search.FillAsync("l"); // matches "Maya L." and "Lunch at noon?"
            (await visibleRows.CountAsync()).ShouldBe(2);
            (await status.TextContentAsync()).ShouldBe("2 conversations found");

            await search.FillAsync("zzz");
            (await visibleRows.CountAsync()).ShouldBe(0);
            (await status.TextContentAsync()).ShouldBe("0 conversations found");
            (await empty.GetAttributeAsync("hidden")).ShouldBeNull();

            await search.FillAsync("");
            (await visibleRows.CountAsync()).ShouldBe(3);
            (await empty.GetAttributeAsync("hidden")).ShouldNotBeNull();
            (await status.TextContentAsync()).ShouldBe("");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U03
    [Fact]
    public async Task RecipientPicker_UnderTwoCharacters_NeverCallsServer_AndKeepsButtonDisabled()
    {
        var apiCalls = 0;
        var page = await fx.OpenComponentPageAsync(
            "/Messages",
            UiDomFixtures.MessagesPage(),
            "messages.js",
            async p =>
            {
                await p.RouteAsync("**/Messages/FindUsers*", async route =>
                {
                    Interlocked.Increment(ref apiCalls);
                    await route.FulfillAsync(new() { Status = 200, ContentType = "application/json", Body = "[]" });
                });
            });

        try
        {
            await page.Locator("[data-new-message]").ClickAsync();
            await page.Locator("[data-recipient-search]").FillAsync(" a ");
            await page.WaitForTimeoutAsync(350); // past 250ms debounce

            (await page.Locator("[data-recipient-status]").TextContentAsync()).ShouldBe("Enter at least 2 characters.");
            (await page.Locator("[data-start-conversation-button]").IsDisabledAsync()).ShouldBeTrue();
            apiCalls.ShouldBe(0);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U04
    [Fact]
    public async Task RecipientPicker_StaleSlowResponse_NeverOverwritesNewerResults()
    {
        var maStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var marStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var slowMaBody = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fastMarBody = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        var page = await fx.OpenComponentPageAsync(
            "/Messages",
            UiDomFixtures.MessagesPage(),
            "messages.js",
            async p =>
            {
                await p.RouteAsync("**/Messages/FindUsers*", async route =>
                {
                    var url = new Uri(route.Request.Url);
                    var q = System.Web.HttpUtility.ParseQueryString(url.Query)["q"];
                    if (q == "ma")
                    {
                        maStarted.TrySetResult();
                        var body = await slowMaBody.Task;
                        await route.FulfillAsync(new() { Status = 200, ContentType = "application/json", Body = body });
                    }
                    else if (q == "mar")
                    {
                        marStarted.TrySetResult();
                        var body = await fastMarBody.Task;
                        await route.FulfillAsync(new() { Status = 200, ContentType = "application/json", Body = body });
                    }
                });
            });

        try
        {
            await page.Locator("[data-new-message]").ClickAsync();
            var input = page.Locator("[data-recipient-search]");

            await input.FillAsync("ma");
            await maStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

            await input.FillAsync("mar");
            await marStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

            // Newer query ("mar") resolves FIRST; older query ("ma") resolves SECOND.
            fastMarBody.SetResult("""[{"id":"u2","name":"Maria Lopez","program":"Digital Design"}]""");
            slowMaBody.SetResult("""[{"id":"u1","name":"Mallory Q","program":""}]""");

            var option = page.Locator("[data-recipient-results] button");
            await Assertions.Expect(option).ToHaveCountAsync(1);
            (await option.TextContentAsync()).ShouldBe("Maria Lopez · Digital Design");

            await option.ClickAsync();
            (await page.Locator("input[name='recipientId']").InputValueAsync()).ShouldBe("u2");
            (await option.GetAttributeAsync("aria-pressed")).ShouldBe("true");
            (await page.Locator("[data-start-conversation-button]").IsDisabledAsync()).ShouldBeFalse();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U05
    [Fact]
    public async Task StartConversation_BlocksEmptySubmit_AndLocksButtonOnValidSubmit()
    {
        var page = await fx.OpenComponentPageAsync("/Messages", UiDomFixtures.MessagesPage(), "messages.js");
        try
        {
            var result = await page.EvaluateAsync<string[]>("""
                () => {
                    const form = document.querySelector("[data-start-conversation]");
                    const button = document.querySelector("[data-start-conversation-button]");
                    const status = document.querySelector("[data-recipient-status]");
                    const hidden = form.querySelector("input[name='recipientId']");

                    const emptyAllowed = form.dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }));
                    const statusAfterEmpty = status.textContent;

                    // Prevent browser navigation after messages.js submit handler runs
                    form.addEventListener("submit", (e) => e.preventDefault());
                    hidden.value = "u2";
                    button.disabled = false;
                    form.dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }));

                    return [String(emptyAllowed), statusAfterEmpty, String(button.disabled), button.textContent];
                }
                """);

            result[0].ShouldBe("false");
            result[1].ShouldBe("Choose an alumnus first.");
            result[2].ShouldBe("true");
            result[3].ShouldBe("Opening...");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U06
    [Fact]
    public async Task SearchInConversation_HighlightsFromNewest_StepsAndWraps_AndEscapeResets()
    {
        var html = UiDomFixtures.MessagesPage(
            ("Maya L.", "Lunch at noon?"),
            ("You", "Sure, see you at lunch"),
            ("Maya L.", "Bring the notes"),
            ("You", "Lunch is on me"));

        var page = await fx.OpenComponentPageAsync("/Messages", html, "messages.js");
        try
        {
            var opener = page.Locator(".msg-chat-head [data-conversation-search]");
            var panel = page.Locator("[data-chat-search]");
            var box = panel.Locator("[data-chat-search-input]");
            var count = panel.Locator("[data-chat-search-count]");

            await opener.ClickAsync();
            (await panel.GetAttributeAsync("hidden")).ShouldBeNull();
            (await opener.GetAttributeAsync("aria-expanded")).ShouldBe("true");

            await box.FillAsync("lunch");
            (await page.Locator(".msg-bubble.is-match").CountAsync()).ShouldBe(3);
            (await count.TextContentAsync()).ShouldBe("3 of 3");
            (await CurrentMatchIndexAsync(page)).ShouldBe(3); // newest match first

            await box.PressAsync("Enter");
            (await CurrentMatchIndexAsync(page)).ShouldBe(1);
            (await count.TextContentAsync()).ShouldBe("2 of 3");

            await box.PressAsync("Enter");
            await box.PressAsync("Enter"); // wraps from oldest back to newest
            (await CurrentMatchIndexAsync(page)).ShouldBe(3);

            await box.PressAsync("Shift+Enter"); // wraps forward to oldest
            (await CurrentMatchIndexAsync(page)).ShouldBe(0);

            await box.FillAsync("zzz");
            (await count.TextContentAsync()).ShouldBe("No matches");

            await box.FillAsync("lunch");
            await box.PressAsync("Escape");
            (await panel.GetAttributeAsync("hidden")).ShouldNotBeNull();
            (await box.InputValueAsync()).ShouldBe("");
            (await page.Locator(".msg-bubble.is-match").CountAsync()).ShouldBe(0);
            (await opener.GetAttributeAsync("aria-expanded")).ShouldBe("false");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static Task<int> CurrentMatchIndexAsync(IPage page) =>
        page.EvaluateAsync<int>("() => [...document.querySelectorAll('.msg-bubble')].findIndex(b => b.classList.contains('is-current'))");
}
