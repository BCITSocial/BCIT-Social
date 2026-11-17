using Microsoft.Playwright;

namespace BCIT_Social.Tests.Support;

[CollectionDefinition("PlaywrightUi")]
public sealed class PlaywrightUiCollection : ICollectionFixture<PlaywrightFixture>;

/// <summary>
/// Shared headless Chromium browser for Playwright UI unit tests (U01–U10).
/// Serves isolated HTML component fixtures on a virtual origin (http://bcit.local) and injects the real
/// production scripts from wwwroot/js/*.js, so UI scripts run in a real browser engine with zero DB or server setup.
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public const string ORIGIN = "http://bcit.local";

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await LaunchBrowserWithFallbackAsync(Playwright);
    }

    private static async Task<IBrowser> LaunchBrowserWithFallbackAsync(IPlaywright playwright)
    {
        // Try bundled Playwright Chromium first, then fall back to system Chrome or Edge if `playwright install` has not run yet.
        foreach (var channel in new string?[] { null, "chrome", "msedge" })
        {
            try
            {
                return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Channel = channel,
                });
            }
            catch (PlaywrightException)
            {
                // Try next channel
            }
        }

        throw new InvalidOperationException(
            "Could not launch Chromium, Chrome, or Edge for Playwright UI tests. Run `pwsh bin/Debug/net10.0/playwright.ps1 install chromium`.");
    }

    /// <summary>
    /// Opens an isolated browser context + page at <c>http://bcit.local{path}</c>, mounts <paramref name="bodyHtml"/>,
    /// and executes the production script <c>wwwroot/js/{scriptFileName}</c>.
    /// </summary>
    public async Task<IPage> OpenComponentPageAsync(
        string path,
        string bodyHtml,
        string scriptFileName,
        Func<IPage, Task>? configureBeforeGoto = null)
    {
        var context = await Browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var scriptSource = await File.ReadAllTextAsync(TestHelpers.WebProjectPath("wwwroot", "js", scriptFileName));
        var fullUrl = $"{ORIGIN}{path}";

        await page.RouteAsync(fullUrl, async route =>
        {
            var document = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head><meta charset="utf-8"><title>BCIT Social UI Test</title></head>
                <body>
                {{bodyHtml}}
                <script>
                {{scriptSource}}
                </script>
                </body>
                </html>
                """;

            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "text/html; charset=utf-8",
                Body = document,
            });
        });

        if (configureBeforeGoto is not null)
        {
            await configureBeforeGoto(page);
        }

        await page.GotoAsync(fullUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        return page;
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null) await Browser.DisposeAsync();
        Playwright?.Dispose();
    }
}
