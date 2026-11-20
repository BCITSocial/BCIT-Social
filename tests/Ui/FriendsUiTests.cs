using BCIT_Social.Tests.Support;

namespace BCIT_Social.Tests.Ui;

[Collection("PlaywrightUi")]
public class FriendsUiTests(PlaywrightFixture fx)
{
    [Fact]
    public async Task CancelRequest_RequiresConfirmation_AndCanBeDismissed()
    {
        const string html = """
            <form data-cancel-request data-recipient-name="Maya Chen">
                <button type="submit">Cancel request</button>
            </form>
            <dialog data-cancel-dialog>
                <strong data-cancel-recipient></strong>
                <button type="button" data-keep-request>Keep request</button>
                <button type="button" data-confirm-cancel>Confirm cancellation</button>
            </dialog>
            """;
        var page = await fx.OpenComponentPageAsync("/Friends", html, "friends.js");
        try
        {
            await page.EvaluateAsync("""
                () => {
                    window.submissions = 0;
                    document.querySelector('form').addEventListener('submit', event => {
                        if (!event.defaultPrevented) window.submissions++;
                        event.preventDefault();
                    });
                }
                """);
            var dialog = page.Locator("[data-cancel-dialog]");
            var cancelButton = page.Locator("[data-cancel-request] button");

            await cancelButton.ClickAsync();
            (await dialog.IsVisibleAsync()).ShouldBeTrue();
            (await page.Locator("[data-cancel-recipient]").TextContentAsync()).ShouldBe("Maya Chen");
            await page.Locator("[data-keep-request]").ClickAsync();
            (await dialog.IsVisibleAsync()).ShouldBeFalse();
            (await page.EvaluateAsync<int>("() => window.submissions")).ShouldBe(0);

            await cancelButton.ClickAsync();
            await page.Keyboard.PressAsync("Escape");
            (await dialog.IsVisibleAsync()).ShouldBeFalse();
            (await page.EvaluateAsync<int>("() => window.submissions")).ShouldBe(0);

            await cancelButton.ClickAsync();
            await page.Locator("[data-confirm-cancel]").ClickAsync();
            (await page.EvaluateAsync<int>("() => window.submissions")).ShouldBe(1);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task RemoveFriend_SubmitsOnlyWhenConfirmed(bool confirm, int expectedSubmissions)
    {
        const string html = """
            <form data-remove-friend data-friend-name="Maya Chen">
                <button type="submit">Remove</button>
            </form>
            """;
        var page = await fx.OpenComponentPageAsync("/Friends", html, "friends.js");
        try
        {
            await page.EvaluateAsync("""
                () => {
                    window.submissions = 0;
                    document.querySelector('form').addEventListener('submit', event => {
                        if (!event.defaultPrevented) window.submissions++;
                        event.preventDefault();
                    });
                }
                """);
            string? confirmationMessage = null;
            page.Dialog += async (_, dialog) =>
            {
                confirmationMessage = dialog.Message;
                if (confirm) await dialog.AcceptAsync();
                else await dialog.DismissAsync();
            };

            await page.GetByRole(Microsoft.Playwright.AriaRole.Button).ClickAsync();

            confirmationMessage.ShouldBe("Remove Maya Chen from your friends?");
            (await page.EvaluateAsync<int>("() => window.submissions")).ShouldBe(expectedSubmissions);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
