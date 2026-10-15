namespace BCITSocial.FrontEnd.Models;

// Data for the whole Events page: the current filters, the dropdown options, and the cards.
public class EventsViewModel
{
    public const string Discover = "discover";
    public const string Calendar = "calendar";

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

    // Options for the Community dropdown. Backend fills this in.
    public IReadOnlyList<string> Communities { get; set; } = [];
    // The cards to show. Backend fills this in.
    public IReadOnlyList<EventCardViewModel> Events { get; set; } = [];

    // Copies the URL values into the filters. A missing or invalid value falls back to a default.
    public EventsViewModel(EventsQuery query)
    {
        Query = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();
        Community = string.IsNullOrWhiteSpace(query.Community) ? null : query.Community;
        Campus = Campuses.FirstOrDefault(c => c == query.Campus);
        Range = Pick(Ranges, query.Range, "month");
        Sort = Pick(Sorts, query.Sort, "soonest");
        Status = Pick(Statuses, query.Status, "all");
        View = Pick(Views, query.View, Discover);
    }

    // Builds link parameters that keep every current filter and change only the status or the view.
    // The status chips and the tabs use this so clicking one does not reset the others.
    public Dictionary<string, string> RouteValues(string? status = null, string? view = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["q"] = Query,
            ["community"] = Community,
            ["campus"] = Campus,
            ["range"] = Range,
            ["sort"] = Sort,
            ["status"] = status ?? Status,
            ["view"] = view ?? View,
        };
        return values.Where(v => !string.IsNullOrEmpty(v.Value)).ToDictionary(v => v.Key, v => v.Value!);
    }

    // Returns value if it is one of the options, otherwise fallback.
    private static string Pick(IReadOnlyList<(string Value, string Label)> options, string? value, string fallback)
        => options.Any(o => o.Value == value) ? value! : fallback;
}
