using BCIT_Social.Tests.Support;
using Microsoft.Playwright;

namespace BCIT_Social.Tests.Ui;

[Collection("PlaywrightUi")]
public class CommunityMembersUiTests(PlaywrightFixture fx)
{
    [Fact]
    public async Task MembersModal_OpensViaInvokerButton_AndDismissesWithCloseAndEscape()
    {
        const string html = """
            <button type="button" command="show-modal" commandfor="community-members-modal">
                3 members
            </button>
            <dialog id="community-members-modal" class="members-dialog" closedby="any" aria-labelledby="members-modal-title">
                <h1 id="members-modal-title">Members</h1>
                <button type="button" command="close" commandfor="community-members-modal" aria-label="Close members" autofocus>
                    Close
                </button>
            </dialog>
            """;

        var page = await fx.OpenComponentPageAsync(
            "/Community/Details/1",
            html,
            "community-details.js",
            p => p.RouteAsync("https://esm.run/**", route => route.AbortAsync()));

        try
        {
            var dialog = page.Locator("#community-members-modal");
            var openButton = page.Locator("[command='show-modal'][commandfor='community-members-modal']");
            var closeButton = page.Locator("[command='close'][commandfor='community-members-modal']");

            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeFalse();

            await openButton.ClickAsync();
            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();
            (await dialog.IsVisibleAsync()).ShouldBeTrue();

            await closeButton.ClickAsync();
            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeFalse();

            await openButton.ClickAsync();
            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();

            await page.Keyboard.PressAsync("Escape");
            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeFalse();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task MembersModal_OpensFromHash_AndClearsHashOnClose()
    {
        const string html = """
            <dialog id="community-members-modal" class="members-dialog" closedby="any" aria-labelledby="members-modal-title">
                <h1 id="members-modal-title">Members</h1>
                <button type="button" command="close" commandfor="community-members-modal" aria-label="Close members" autofocus>
                    Close
                </button>
            </dialog>
            """;

        var page = await fx.OpenComponentPageAsync(
            "/Community/Details/1",
            html,
            "community-details.js",
            p => p.RouteAsync("https://esm.run/**", route => route.AbortAsync()));

        try
        {
            await page.GotoAsync("about:blank");
            await page.GotoAsync(
                $"{PlaywrightFixture.ORIGIN}/Community/Details/1#members",
                new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            var dialog = page.Locator("#community-members-modal");
            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();
            (await page.EvaluateAsync<string>("() => window.location.hash")).ShouldBe("#members");

            await page.Keyboard.PressAsync("Escape");
            (await dialog.EvaluateAsync<bool>("d => d.open")).ShouldBeFalse();
            await page.WaitForFunctionAsync("() => window.location.hash === ''");
            (await page.EvaluateAsync<string>("() => window.location.hash")).ShouldBe("");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task MembersModal_ConfirmAction_PostsFormAndReopensModalOnDetails()
    {
        const string html = """
            <dialog id="community-members-modal" class="members-dialog" closedby="any" aria-labelledby="members-modal-title">
                <h1 id="members-modal-title">Members</h1>
                <form action="/Community/Kick/1/u2" method="post"
                      class="member-action-form" data-member-action-form
                      data-confirm-title="Remove member?"
                      data-confirm-message="Remove Ava Williams from this community? They can join again later."
                      data-confirm-button="Remove"
                      data-confirm-cancel="Cancel">
                    <input type="hidden" name="Id" value="u2" />
                    <input type="hidden" name="DisplayName" value="Ava Williams" />
                    <input type="hidden" name="IsModerator" value="false" />
                    <input type="hidden" name="CommunityName" value="BCIT Developers Network" />
                    <input type="hidden" name="CommunityId" value="1" />
                    <button type="submit" class="member-action-btn is-danger">Remove</button>
                </form>
            </dialog>
            <dialog id="community-confirm" class="community-dialog confirm-dialog" aria-labelledby="community-confirm-title"
                    aria-describedby="community-confirm-message">
                <form method="post" class="modal-card confirm-card" data-confirm-form>
                    <h2 id="community-confirm-title" class="modal-title" data-confirm-title></h2>
                    <p id="community-confirm-message" class="modal-subtitle" data-confirm-message></p>
                    <div class="modal-footer-actions">
                        <button type="button" class="btn-cancel" data-close-modal data-confirm-cancel></button>
                        <button type="submit" class="btn-danger-confirm" data-confirm-submit></button>
                    </div>
                </form>
            </dialog>
            """;

        var kickPosts = new List<string>();
        var page = await fx.OpenComponentPageAsync(
            "/Community/Details/1",
            html,
            "community-details.js",
            async p =>
            {
                await p.RouteAsync("https://esm.run/**", route => route.AbortAsync());
                await p.RouteAsync($"{PlaywrightFixture.ORIGIN}/Community/Kick/1/u2", async route =>
                {
                    if (route.Request.Method == "POST")
                    {
                        kickPosts.Add(route.Request.PostData ?? "");
                    }
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "text/html; charset=utf-8",
                        Body = "OK",
                    });
                });
            });

        try
        {
            var membersDialog = page.Locator("#community-members-modal");
            var confirmDialog = page.Locator("#community-confirm");

            await page.EvaluateAsync("() => document.getElementById('community-members-modal').showModal()");
            (await membersDialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();

            await membersDialog.Locator("button[type='submit']").ClickAsync();
            (await confirmDialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();
            (await membersDialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();
            (await confirmDialog.Locator("[data-confirm-title]").TextContentAsync()).ShouldBe("Remove member?");

            await confirmDialog.Locator("[data-confirm-cancel]").ClickAsync();
            (await confirmDialog.EvaluateAsync<bool>("d => d.open")).ShouldBeFalse();
            (await membersDialog.EvaluateAsync<bool>("d => d.open")).ShouldBeTrue();
            kickPosts.Count.ShouldBe(0);

            await page.EvaluateAsync("() => { window.__beforeReload = true; }");
            await membersDialog.Locator("button[type='submit']").ClickAsync();
            await confirmDialog.Locator("[data-confirm-submit]").ClickAsync();
            await page.WaitForFunctionAsync(
                "() => !window.__beforeReload && window.location.hash === '#members' && document.getElementById('community-members-modal')?.open === true");

            kickPosts.Count.ShouldBe(1);
            kickPosts[0].ShouldContain("name=\"Id\"\r\n\r\nu2");
            kickPosts[0].ShouldContain("name=\"DisplayName\"\r\n\r\nAva Williams");
            kickPosts[0].ShouldContain("name=\"CommunityName\"\r\n\r\nBCIT Developers Network");
            kickPosts[0].ShouldContain("name=\"CommunityId\"\r\n\r\n1");
            (await page.EvaluateAsync<string>("() => window.location.pathname")).ShouldBe("/Community/Details/1");
            (await page.EvaluateAsync<string>("() => window.location.hash")).ShouldBe("#members");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
