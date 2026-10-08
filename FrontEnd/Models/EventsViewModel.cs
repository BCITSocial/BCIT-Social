namespace BCITSocial.FrontEnd.Models;

public class EventsViewModel
{
    public const string Upcoming = "upcoming";
    public const string ThisWeek = "week";
    public const string Past = "past";

    public static readonly IReadOnlyList<(string Value, string Label)> Scopes =
    [
        (Upcoming, "Upcoming"),
        (ThisWeek, "This week"),
        (Past, "Past"),
    ];

    public static readonly IReadOnlyList<string> Campuses =
    [
        "Burnaby",
        "Downtown",
        "Aerospace",
        "Marine",
        "Annacis Island",
    ];

    public string Scope { get; }
    public string? Campus { get; }
    public IReadOnlyList<EventCardViewModel> Events { get; init; } = [];

    public EventsViewModel(string? scope = null, string? campus = null)
    {
        Scope = Scopes.Any(s => s.Value == scope) ? scope! : Upcoming;
        Campus = Campuses.FirstOrDefault(c => c == campus);
    }
}
