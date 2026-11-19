namespace BCIT_Social.ViewModels;

// One row in the Upcoming events card. A line is left out when its value is empty.
public class RailEventViewModel
{
    // Used for the link to the event.
    public int Id { get; init; }
    public string Title { get; init; } = "";
    // The date tile and the "Friday, 6:00 PM" line come from StartsAt.
    public DateTime? StartsAt { get; init; }
    // Room or venue, for example "SE2 Lab". Shown together with Campus.
    public string Location { get; init; } = "";
    public string Campus { get; init; } = "";
    public string Description { get; init; } = "";
    public int GoingCount { get; init; }
    // The row shows the first three as avatars.
    public IReadOnlyList<AttendeeViewModel> Attendees { get; init; } = [];
    // The signed-in user's relationship to this event. Going shows the button as "Going".
    public RsvpStatus Rsvp { get; init; }
}
