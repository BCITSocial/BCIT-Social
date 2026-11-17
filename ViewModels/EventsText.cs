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

    // Filter menu button. {0} in ActiveFilters is the number of settings changed from the default;
    // screen readers hear it after the button text, e.g. "Filter & sort, 2 filters on".
    // ActiveFiltersOne is used when exactly one is on.
    public string FilterMenuButton { get; init; } = "Filter & sort";
    public string ActiveFilters { get; init; } = "{0} filters on";
    public string ActiveFiltersOne { get; init; } = "{0} filter on";
    public string ResetFilters { get; init; } = "Reset filters";

    // Filter menu fields. {0} in SortOption is replaced with the sort label. The Sort label above the dropdown names it,
    // so the option shows just the label ("Soonest").
    public string AllCommunities { get; init; } = "All communities";
    public string AllCampuses { get; init; } = "All campuses";
    public string SortOption { get; init; } = "{0}";
    public string Apply { get; init; } = "Apply";

    // Tag chips above the cards. AllTags is the first chip, which shows every tag.
    public string LabelTags { get; init; } = "Browse by tag";
    public string AllTags { get; init; } = "All";

    // Calendar tab. {0} in CalendarMore is how many more events that day has than fit in the box.
    // Weekdays are the column headings, Sunday first.
    public string CalendarPrevious { get; init; } = "Previous month";
    public string CalendarNext { get; init; } = "Next month";
    public string CalendarToday { get; init; } = "Today";
    public string CalendarMore { get; init; } = "+{0} more";
    public List<string> Weekdays { get; init; } = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    // Everything in the "Create event" pop-up (see CreateEventText).
    public CreateEventText CreateEvent { get; init; } = new();

    // Messages shown instead of the card grid.
    public string EmptyEvents { get; init; } = "No events found.";
    public string EmptyCalendar { get; init; } = "No events this month.";

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
    // Bookmark icon on each card. Screen readers hear this text; the icon shows filled when the event is saved.
    public string ButtonBookmark { get; init; } = "Save event";

    // Labels only screen readers hear (visually hidden or aria-label).
    public string LabelMyEvents { get; init; } = "My events";
    public string LabelEventsView { get; init; } = "Events view";
    public string LabelCommunity { get; init; } = "Community";
    public string LabelCampus { get; init; } = "Campus";
    public string LabelRange { get; init; } = "Time range";
    public string LabelSort { get; init; } = "Sort";
    public string LabelHost { get; init; } = "Host";
    public string LabelTime { get; init; } = "Time";
    public string LabelLocation { get; init; } = "Location";
    public string LabelEventTags { get; init; } = "Tags";

    // Label for an option from the JSON dictionary, or the fallback label from EventsViewModel.
    public static string Pick(Dictionary<string, string>? labels, string value, string fallback)
        => labels is not null && labels.TryGetValue(value, out var label) && !string.IsNullOrWhiteSpace(label) ? label : fallback;

    // Fills {0} in a pattern such as GoingCount. A pattern without {0} is shown as written.
    public static string Format(string pattern, object value)
        => pattern.Contains("{0}") ? pattern.Replace("{0}", value.ToString()) : pattern;
}
