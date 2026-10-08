namespace BCITSocial.FrontEnd.Models;

public class EventCardViewModel
{
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public DateTime StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public string Campus { get; init; } = "";
    public string Location { get; init; } = "";
    public string HostName { get; init; } = "";
    public bool IsCancelled { get; init; }
}
