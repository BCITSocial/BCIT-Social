using BCIT_Social.Tests.Support;
using Microsoft.Playwright;

namespace BCIT_Social.Tests.Ui;

/// <summary>
/// Pure UI unit tests for wwwroot/js/feed.js running in headless Chromium via Playwright (U09–U10).
/// Verifies the 10-card look-ahead target, keyset cursor handoff (beforeTicks + beforeId), X-Feed-Has-More termination,
/// in-flight concurrency guard, and #feed-retry error recovery.
/// </summary>
[Collection("PlaywrightUi")]
public class FeedUiTests(PlaywrightFixture fx)
{
    private const string INTERSECTION_OBSERVER_SPY_INIT_SCRIPT = """
        window.__observers = [];
        window.IntersectionObserver = class {
            constructor(callback) {
                this.callback = callback;
                this.targets = [];
                window.__observers.push(this);
            }
            observe(target) { this.targets.push(target); }
            disconnect() { this.targets = []; }
        };
        window.__observedPostIds = () => window.__observers[0]?.targets.map(t => t.dataset.postId) ?? [];
        window.__triggerScroll = () => window.__observers[0]?.callback([{ isIntersecting: true, boundingClientRect: { top: 0 } }]);
        """;

    // U09
    [Fact]
    public async Task InfiniteScroll_WatchesTenthCardFromEnd_SendsLastCardCursor_AppendsCards_AndStopsWhenCaughtUp()
    {
        var requestedUrls = new List<string>();
        var requestedHeaders = new List<string?>();
        var callCount = 0;

        var page = await fx.OpenComponentPageAsync(
            "/",
            UiDomFixtures.FeedPage(12),
            "feed.js",
            async p =>
            {
                await p.AddInitScriptAsync(INTERSECTION_OBSERVER_SPY_INIT_SCRIPT);
                await p.RouteAsync("**/Home/Feed*", async route =>
                {
                    requestedUrls.Add(route.Request.Url);
                    requestedHeaders.Add(route.Request.Headers.GetValueOrDefault("x-requested-with"));
                    var call = Interlocked.Increment(ref callCount);

                    if (call == 1)
                    {
                        await route.FulfillAsync(new()
                        {
                            Status = 200,
                            ContentType = "text/html",
                            Headers = new Dictionary<string, string> { ["X-Feed-Has-More"] = "true" },
                            Body = UiDomFixtures.PostCard(80, 5) + UiDomFixtures.PostCard(79, 4),
                        });
                    }
                    else
                    {
                        await route.FulfillAsync(new()
                        {
                            Status = 200,
                            ContentType = "text/html",
                            Headers = new Dictionary<string, string> { ["X-Feed-Has-More"] = "false" },
                            Body = UiDomFixtures.PostCard(70, 3),
                        });
                    }
                });
            });

        try
        {
            // 12 cards (ids 100..89): 10th from the end is index 2 -> id "98"
            (await page.EvaluateAsync<string[]>("() => window.__observedPostIds()")).ShouldBe(["98"]);

            await page.EvaluateAsync("() => window.__triggerScroll()");
            await Assertions.Expect(page.Locator(".post-card")).ToHaveCountAsync(14);

            requestedUrls[0].ShouldContain("beforeId=89"); // last card of the initial 12
            requestedUrls[0].ShouldContain("beforeTicks=638999999999999989");
            requestedHeaders[0].ShouldBe("XMLHttpRequest");
            (await page.Locator("#feed-status").TextContentAsync()).ShouldBe("");
            (await page.EvaluateAsync<string[]>("() => window.__observedPostIds()")).Length.ShouldBe(1);

            await page.EvaluateAsync("() => window.__triggerScroll()");
            await Assertions.Expect(page.Locator(".post-card")).ToHaveCountAsync(15);

            requestedUrls[1].ShouldContain("beforeId=79"); // cursor advanced to the new last card
            (await page.Locator("#feed-status").TextContentAsync()).ShouldBe("You're all caught up.");
            (await page.EvaluateAsync<string[]>("() => window.__observedPostIds()")).ShouldBeEmpty();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    // U10
    [Fact]
    public async Task InfiniteScroll_FailedRequestShowsRetry_AndConcurrentTriggersSendOnlyOneRequest()
    {
        var callCount = 0;
        var firstRequestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishFirstRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var page = await fx.OpenComponentPageAsync(
            "/",
            UiDomFixtures.FeedPage(12),
            "feed.js",
            async p =>
            {
                await p.AddInitScriptAsync(INTERSECTION_OBSERVER_SPY_INIT_SCRIPT);
                await p.RouteAsync("**/Home/Feed*", async route =>
                {
                    var call = Interlocked.Increment(ref callCount);
                    if (call == 1)
                    {
                        firstRequestStarted.TrySetResult();
                        await finishFirstRequest.Task;
                        await route.FulfillAsync(new() { Status = 500, Body = "Server error" });
                    }
                    else
                    {
                        await route.FulfillAsync(new()
                        {
                            Status = 200,
                            ContentType = "text/html",
                            Headers = new Dictionary<string, string> { ["X-Feed-Has-More"] = "true" },
                            Body = UiDomFixtures.PostCard(60, 2),
                        });
                    }
                });
            });

        try
        {
            // Trigger scroll once, wait for request #1 to be in flight, then trigger scroll a second time.
            await page.EvaluateAsync("() => window.__triggerScroll()");
            await firstRequestStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await page.EvaluateAsync("() => window.__triggerScroll()");

            finishFirstRequest.SetResult();

            var status = page.Locator("#feed-status");
            var retry = page.Locator("#feed-retry");
            await Assertions.Expect(status).ToHaveTextAsync("Couldn't load more posts.");

            callCount.ShouldBe(1); // guarded by the `loading` flag
            (await retry.GetAttributeAsync("hidden")).ShouldBeNull();

            await retry.ClickAsync();
            await Assertions.Expect(page.Locator(".post-card")).ToHaveCountAsync(13);

            callCount.ShouldBe(2);
            (await retry.GetAttributeAsync("hidden")).ShouldNotBeNull();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
