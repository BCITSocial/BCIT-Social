using BCIT_Social.Tests.Support;
using Microsoft.Playwright;

namespace BCIT_Social.Tests.Ui;

/// <summary>
/// Pure UI unit tests for wwwroot/js/profile-sections.js running in headless Chromium via Playwright (U07–U08).
/// Verifies WAI-ARIA tab selection, roving tabindex, panel visibility, URL ?section= deep-linking, and popstate.
/// </summary>
[Collection("PlaywrightUi")]
public class ProfileSectionsUiTests(PlaywrightFixture fx)
{
    // U07
    [Fact]
    public async Task ClickingTab_SelectsOneTab_ShowsItsPanel_UpdatesUrl_AndFollowsPopstate()
    {
        var page = await fx.OpenComponentPageAsync("/Profile", UiDomFixtures.ProfileTabs(), "profile-sections.js");
        try
        {
            await page.Locator("[data-profile-tab='bookmarks']").ClickAsync();

            (await SelectedTabsAsync(page)).ShouldBe(["bookmarks"]);
            (await VisiblePanelsAsync(page)).ShouldBe(["bookmarks"]);
            (await page.Locator("[data-profile-tab='bookmarks']").GetAttributeAsync("tabindex")).ShouldBe("0");
            (await page.Locator("[data-profile-tab='posts']").GetAttributeAsync("tabindex")).ShouldBe("-1");
            (await page.EvaluateAsync<string>("() => window.location.search")).ShouldBe("?section=bookmarks");

            // Simulate browser Back/Forward to ?section=friends
            await page.EvaluateAsync("""
                () => {
                    history.replaceState(null, "", "/Profile?section=friends");
                    window.dispatchEvent(new PopStateEvent("popstate"));
                }
                """);
            (await SelectedTabsAsync(page)).ShouldBe(["friends"]);
            (await VisiblePanelsAsync(page)).ShouldBe(["friends"]);

            // Simulate browser Back to default /Profile (no query string)
            await page.EvaluateAsync("""
                () => {
                    history.replaceState(null, "", "/Profile");
                    window.dispatchEvent(new PopStateEvent("popstate"));
                }
                """);
            (await SelectedTabsAsync(page)).ShouldBe(["posts"]);
            (await VisiblePanelsAsync(page)).ShouldBe(["posts"]);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U08
    [Fact]
    public async Task KeyboardNavigation_ArrowsMoveAndWrap_HomeAndEndJump_FocusFollows_AndTabKeyIsNotTrapped()
    {
        var page = await fx.OpenComponentPageAsync("/Profile", UiDomFixtures.ProfileTabs(), "profile-sections.js");
        try
        {
            var posts = page.Locator("[data-profile-tab='posts']");
            var bookmarks = page.Locator("[data-profile-tab='bookmarks']");
            var friends = page.Locator("[data-profile-tab='friends']");

            await posts.FocusAsync();
            await posts.PressAsync("ArrowRight");
            (await SelectedTabsAsync(page)).ShouldBe(["bookmarks"]);
            (await ActiveTabNameAsync(page)).ShouldBe("bookmarks");

            await bookmarks.PressAsync("End");
            (await SelectedTabsAsync(page)).ShouldBe(["friends"]);
            (await ActiveTabNameAsync(page)).ShouldBe("friends");

            await friends.PressAsync("ArrowRight"); // wraps forward to first tab
            (await SelectedTabsAsync(page)).ShouldBe(["posts"]);

            await posts.PressAsync("ArrowLeft"); // wraps backward to last tab
            (await SelectedTabsAsync(page)).ShouldBe(["friends"]);

            await friends.PressAsync("Home");
            (await SelectedTabsAsync(page)).ShouldBe(["posts"]);

            var tabPrevented = await page.EvaluateAsync<bool>("""
                () => {
                    const tab = document.querySelector("[data-profile-tab='posts']");
                    const ev = new KeyboardEvent("keydown", { key: "Tab", bubbles: true, cancelable: true });
                    tab.dispatchEvent(ev);
                    return ev.defaultPrevented;
                }
                """);
            tabPrevented.ShouldBeFalse();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static Task<string[]> SelectedTabsAsync(IPage page) =>
        page.EvaluateAsync<string[]>(
            "() => [...document.querySelectorAll('[data-profile-tab]')].filter(t => t.getAttribute('aria-selected') === 'true').map(t => t.dataset.profileTab)");

    private static Task<string[]> VisiblePanelsAsync(IPage page) =>
        page.EvaluateAsync<string[]>(
            "() => [...document.querySelectorAll('[data-profile-panel]')].filter(p => !p.hidden).map(p => p.dataset.profilePanel)");

    private static Task<string?> ActiveTabNameAsync(IPage page) =>
        page.EvaluateAsync<string?>("() => document.activeElement?.dataset?.profileTab ?? null");
}
