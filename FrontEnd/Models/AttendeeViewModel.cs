namespace BCITSocial.FrontEnd.Models;

// One attendee(name and photo URL). If there is no photo, 
// the avatar shows the person's initials(Initials).
public class AttendeeViewModel
{
    public string Name { get; init; } = "";
    public string? PhotoUrl { get; init; }

    public string Initials => string.Concat(
        Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));
}
