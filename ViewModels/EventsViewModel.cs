namespace BCIT_Social.ViewModels;

// Data for the whole Events page: the current filters, the dropdown options, and the cards.
public class EventsViewModel
{
    public const string Discover = "discover";
    public const string Calendar = "calendar";

    // The time range and sort used when the URL does not pick one.
    public const string DefaultRange = "month";
    public const string DefaultSort = "soonest";

    // Option lists for the page. Value is what goes in the URL, Label is what the user sees.
    public static readonly IReadOnlyList<(string Value, string Label)> Views =
    [
        (Discover, "Discover events"),
        (Calendar, "My calendar"),
    ];

    public static readonly IReadOnlyList<(string Value, string Label)> Statuses =
    [
        ("all", "All events"),
        ("interested", "Interested"),
        ("going", "Going"),
        ("hosting", "Hosting"),
        ("saved", "Saved"),
    ];

    public static readonly IReadOnlyList<(string Value, string Label)> Ranges =
    [
        ("week", "This week"),
        ("month", "This month"),
        ("upcoming", "All upcoming"),
        ("past", "Past"),
    ];

    public static readonly IReadOnlyList<(string Value, string Label)> Sorts =
    [
        ("soonest", "Soonest"),
        ("latest", "Latest"),
        ("popular", "Most popular"),
    ];

    public static readonly IReadOnlyList<string> Campuses =
    [
        "Burnaby",
        "Downtown",
        "Aerospace",
        "Marine",
        "Annacis Island",
    ];

    // The current filters, already cleaned up (see the constructor). Backend should filter events with these.
    public string? Query { get; }
    public string? Community { get; }
    public string? Campus { get; }
    public string Range { get; }
    public string Sort { get; }
    public string Status { get; }
    public string View { get; }
    // The selected tag chip, or null for "All". Only a tag from Tags counts, so a made-up tag in the URL is ignored.
    public string? Tag => Tags.FirstOrDefault(t => string.Equals(t, _requestedTag, StringComparison.OrdinalIgnoreCase));
    private readonly string? _requestedTag;
    // First day of the month shown in the calendar tab. This month when the URL does not pick a valid one.
    public DateOnly Month { get; }

    // All wording on the page, from events-text.json. Stays the same when Backend takes over Events.
    public EventsText Text { get; set; } = new();
    // Options for the Community dropdown. Backend fills this in.
    public IReadOnlyList<string> Communities { get; set; } = [];
    // Tag chips above the cards. Backend fills this in.
    public IReadOnlyList<string> Tags { get; set; } = [];
    // The cards to show. Backend fills this in.
    public IReadOnlyList<EventCardViewModel> Events { get; set; } = [];

    // Copies the URL values into the filters. A missing or invalid value falls back to a default.
    public EventsViewModel(EventsQuery query)
    {
        Query = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();
        Community = string.IsNullOrWhiteSpace(query.Community) ? null : query.Community;
        Campus = Campuses.FirstOrDefault(c => c == query.Campus);
        Range = Pick(Ranges, query.Range, DefaultRange);
        Sort = Pick(Sorts, query.Sort, DefaultSort);
        Status = Pick(Statuses, query.Status, "all");
        View = Pick(Views, query.View, Discover);
        _requestedTag = string.IsNullOrWhiteSpace(query.Tag) ? null : query.Tag.Trim();
        var today = DateOnly.FromDateTime(DateTime.Today);
        Month = DateOnly.TryParseExact((query.Month ?? "") + "-01", "yyyy-MM-dd", out var month) ? month : new DateOnly(today.Year, today.Month, 1);
    }

    // The "month" URL value for a month, e.g. "2026-11".
    public static string MonthValue(DateOnly month) => month.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);

    // Builds link parameters that keep every current filter and change only the status, the view or the tag.
    // The status chips, the tabs and the tag chips use this so clicking one does not reset the others.
    // For the tag, pass "" to clear it (the "All" chip) and null to keep the current one.
    // The calendar month is kept only on the calendar tab; pass month to move to another month.
    public Dictionary<string, string> RouteValues(string? status = null, string? view = null, string? tag = null, DateOnly? month = null)
    {
        var nextView = view ?? View;
        var values = new Dictionary<string, string?>
        {
            ["q"] = Query,
            ["community"] = Community,
            ["campus"] = Campus,
            ["range"] = Range,
            ["sort"] = Sort,
            ["status"] = status ?? Status,
            ["view"] = nextView,
            ["tag"] = tag ?? Tag,
            ["month"] = nextView == Calendar && (month is not null || Month != ThisMonth) ? MonthValue(month ?? Month) : null,
        };
        return values.Where(v => !string.IsNullOrEmpty(v.Value)).ToDictionary(v => v.Key, v => v.Value!);
    }

    // How many filter menu settings differ from the defaults. Shown as a number on the menu button.
    public int ActiveFilterCount =>
        (Community is null ? 0 : 1) + (Campus is null ? 0 : 1) + (Range == DefaultRange ? 0 : 1) + (Sort == DefaultSort ? 0 : 1);

    // Link parameters for "Reset filters": keeps the search, the status chip, the tab and the tag chip, drops the menu settings.
    public Dictionary<string, string> ResetRouteValues()
    {
        var values = new Dictionary<string, string?> { ["q"] = Query, ["status"] = Status, ["view"] = View, ["tag"] = Tag };
        return values.Where(v => !string.IsNullOrEmpty(v.Value)).ToDictionary(v => v.Key, v => v.Value!);
    }

    private static DateOnly ThisMonth => new(DateTime.Today.Year, DateTime.Today.Month, 1);

    // Returns value if it is one of the options, otherwise fallback.
    private static string Pick(IReadOnlyList<(string Value, string Label)> options, string? value, string fallback)
        => options.Any(o => o.Value == value) ? value! : fallback;
}
