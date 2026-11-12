namespace BCIT_Social.ViewModels;

// Every piece of text on the Events page, read from the "text" section of wwwroot/content/events-text.json.
// The views only place these values; they hold no wording of their own.
// Each default below is used when the JSON leaves that key out, so a missing key never leaves a slot empty.
public class EventsText
{
    // Browser tab title.
    public string PageTitle { get; init; } = "Events";

    // Banner at the top of the page.
    public string BannerLabel { get; init; } = "Events introduction";
    public string BannerTitle { get; init; } = "Find your next campus connection.";
    public string BannerText { get; init; } = "Discover events from across your BCIT communities.";

    // Panel heading and the line under it.
    public string Heading { get; init; } = "Events";
    public string Subheading { get; init; } = "Find events from across your BCIT communities.";

    // Labels for the status chips, tabs, time ranges and sort options, keyed by the value used in the URL
    // (see EventsViewModel). A value missing here falls back to the label in EventsViewModel.
    public Dictionary<string, string> Statuses { get; init; } = [];
    public Dictionary<string, string> Views { get; init; } = [];
    public Dictionary<string, string> Ranges { get; init; } = [];
    public Dictionary<string, string> Sorts { get; init; } = [];

    // Filter bar. {0} in SortOption is replaced with the sort label, e.g. "Sort: Soonest".
    public string SearchPlaceholder { get; init; } = "Search events...";
    public string AllCommunities { get; init; } = "All communities";
    public string AllCampuses { get; init; } = "All campuses";
    public string SortOption { get; init; } = "Sort: {0}";
    public string Apply { get; init; } = "Apply";

    // Messages shown instead of the card grid.
    public string EmptyEvents { get; init; } = "No events found.";
    public string EmptyCalendar { get; init; } = "Your calendar will appear here.";

    // Card badges and buttons. {0} in GoingCount is replaced with the number of people.
    public string BadgeGoing { get; init; } = "Going";
    public string BadgeInterested { get; init; } = "Interested";
    public string BadgeHosting { get; init; } = "Hosting";
    public string BadgeCancelled { get; init; } = "Cancelled";
    public string GoingCount { get; init; } = "{0} going";
    public string ButtonInterested { get; init; } = "Interested";
    public string ButtonGoing { get; init; } = "Going";
    public string ButtonViewEvent { get; init; } = "View event";
    public string ButtonManageEvent { get; init; } = "Manage event";

    // Labels only screen readers hear (visually hidden or aria-label).
    public string LabelMyEvents { get; init; } = "My events";
    public string LabelEventsView { get; init; } = "Events view";
    public string LabelSearch { get; init; } = "Search events";
    public string LabelCommunity { get; init; } = "Community";
    public string LabelCampus { get; init; } = "Campus";
    public string LabelRange { get; init; } = "Time range";
    public string LabelSort { get; init; } = "Sort";
    public string LabelHost { get; init; } = "Host";
    public string LabelTime { get; init; } = "Time";
    public string LabelLocation { get; init; } = "Location";

    // Label for an option from the JSON dictionary, or the fallback label from EventsViewModel.
    public static string Pick(Dictionary<string, string>? labels, string value, string fallback)
        => labels is not null && labels.TryGetValue(value, out var label) && !string.IsNullOrWhiteSpace(label) ? label : fallback;

    // Fills {0} in a pattern such as GoingCount. A pattern without {0} is shown as written.
    public static string Format(string pattern, object value)
        => pattern.Contains("{0}") ? pattern.Replace("{0}", value.ToString()) : pattern;
}
