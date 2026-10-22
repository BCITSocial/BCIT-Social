namespace BCIT_Social.ViewModels;

// Data for one event card.
public class EventCardViewModel
{
    // Used later for the "View event" and "Manage event" links.
    public int Id { get; init; }
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
    public int GoingCount { get; init; }
    // The card shows the first three as avatars.
    public IReadOnlyList<AttendeeViewModel> Attendees { get; init; } = [];
    // The signed-in user's relationship to this event.
    public RsvpStatus Rsvp { get; init; }
    // Shows a "Cancelled" badge and mutes the card.
    public bool IsCancelled { get; init; }
}
