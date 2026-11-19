namespace BCIT_Social.ViewModels;

// Every piece of text in the right rail, read from the "text" section of wwwroot/content/right-rail-text.json.
// The views only place these values; they hold no wording of their own.
// Each default below is used when the JSON leaves that key out, so a missing key never leaves a slot empty.
public class RightRailText
{
    // Screen reader name of the whole rail.
    public string RailLabel { get; init; } = "More on BCIT Social";

    // Upcoming events card. {0} in GoingCount is replaced with the number of people.
    public string EventsHeading { get; init; } = "Upcoming Events";
    public string EventsLink { get; init; } = "See all";
    public string EventsEmpty { get; init; } = "No upcoming events yet.";
    public string GoingCount { get; init; } = "{0} going";
    public string ButtonRsvp { get; init; } = "RSVP";
    public string ButtonGoing { get; init; } = "Going";

    // Quick links card. NewTab is read by screen readers after a link that opens in a new tab.
    public string LinksHeading { get; init; } = "Quick links";
    public string LinksEmpty { get; init; } = "No links yet.";
    public string NewTab { get; init; } = "(opens in a new tab)";

    // Fills {0} in a pattern such as GoingCount. A pattern without {0} is shown as written.
    public static string Format(string pattern, object value)
        => pattern.Contains("{0}") ? pattern.Replace("{0}", value.ToString()) : pattern;
}
