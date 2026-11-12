using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Presentation;

/// <summary>
/// The Events page reads its filters from the URL (untrusted). The constructor is the only gate between the query string
/// and the links/JSON the page renders, so every fallback is pinned here.
/// </summary>
public class EventsViewModelTests
{
    // T04
    [Theory]
    //          range sort status view  range     sort      status     view
    [InlineData(null, null, null, null, "month", "soonest", "all", "discover")]
    [InlineData("", "", "", "", "month", "soonest", "all", "discover")]
    [InlineData("bogus", "x", "y", "z", "month", "soonest", "all", "discover")]
    [InlineData("WEEK", "LATEST", "GOING", "CALENDAR", "month", "soonest", "all", "discover")] // values are case-sensitive
    [InlineData("past", "popular", "hosting", "calendar", "past", "popular", "hosting", "calendar")]
    [InlineData("week", "latest", "interested", "discover", "week", "latest", "interested", "discover")]
    public void Constructor_InvalidOrMissingOptions_FallBackToDefaults(
        string? range, string? sort, string? status, string? view,
        string expectedRange, string expectedSort, string expectedStatus, string expectedView)
    {
        var model = new EventsViewModel(new EventsQuery { Range = range, Sort = sort, Status = status, View = view });

        model.Range.ShouldBe(expectedRange);
        model.Sort.ShouldBe(expectedSort);
        model.Status.ShouldBe(expectedStatus);
        model.View.ShouldBe(expectedView);
    }

    // T05
    [Theory]
    [InlineData("  robotics  ", "Burnaby", "robotics", "Burnaby")]
    [InlineData("   ", "Downtown", null, "Downtown")]
    [InlineData(null, "Annacis Island", null, "Annacis Island")]
    [InlineData("ai", "burnaby", "ai", null)]                    // campus is matched exactly
    [InlineData("ai", "Mars", "ai", null)]                       // not in the whitelist
    [InlineData("ai", "<script>alert(1)</script>", "ai", null)]  // never echoed back into links
    public void Constructor_TrimsSearchText_AndOnlyAcceptsKnownCampuses(
        string? q, string? campus, string? expectedQuery, string? expectedCampus)
    {
        var model = new EventsViewModel(new EventsQuery { Q = q, Campus = campus });

        model.Query.ShouldBe(expectedQuery);
        model.Campus.ShouldBe(expectedCampus);
    }

    // T06
    [Fact]
    public void RouteValues_KeepsEveryFilter_AndOverridesOnlyWhatWasAsked()
    {
        var model = new EventsViewModel(new EventsQuery
        {
            Q = "ai",
            Community = "Robotics",
            Campus = "Burnaby",
            Range = "week",
            Sort = "latest",
            Status = "going",
            View = "calendar",
        });

        var chip = model.RouteValues(status: "hosting");
        chip["status"].ShouldBe("hosting");
        chip["view"].ShouldBe("calendar");
        chip["q"].ShouldBe("ai");
        chip["community"].ShouldBe("Robotics");
        chip["campus"].ShouldBe("Burnaby");

        var tab = model.RouteValues(view: "discover");
        tab["view"].ShouldBe("discover");
        tab["status"].ShouldBe("going");

        // Empty filters are omitted so URLs stay clean.
        new EventsViewModel(new EventsQuery()).RouteValues().Keys
            .ShouldBe(["range", "sort", "status", "view"], ignoreOrder: true);
    }
}
