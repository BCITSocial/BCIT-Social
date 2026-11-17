namespace BCIT_Social.ViewModels;

// Data for one event card.
public class EventCardViewModel
{
    // Used later for the "View event" and "Manage event" links.
    public int Id { get; init; }
    // Link to the event's detail page. Leave null until that page exists; the calendar then shows the title as plain text.
    public string? DetailsUrl { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    // StartsAt is the event time. EndsAt is optional.
    public DateTime? StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    // Name of the community hosting the event.
    public string HostName { get; init; } = "";
    // Room or venue, for example "SE12". Shown together with Campus.
    public string Location { get; init; } = "";
    public string Campus { get; init; } = "";
    // Leave CoverImageUrl null for no image. Leave CoverImageAlt empty if the image is only decoration.
    public string? CoverImageUrl { get; init; }
    public string CoverImageAlt { get; init; } = "";
    // Tags shown as badges under the title, e.g. "Workshop". Empty means no badge row.
    public IReadOnlyList<string> Tags { get; init; } = [];
    public int GoingCount { get; init; }
    // The card shows the first three as avatars.
    public IReadOnlyList<AttendeeViewModel> Attendees { get; init; } = [];
    // The signed-in user's relationship to this event.
    public RsvpStatus Rsvp { get; init; }
    // True when the signed-in user bookmarked this event. Fills in the bookmark icon on the card.
    public bool IsBookmarked { get; init; }
    // Shows a "Cancelled" badge and mutes the card.
    public bool IsCancelled { get; init; }
}
