namespace BCIT_Social.ViewModels;

// A container that receives the URL's ?q=...&campus=... values.
public class EventsQuery
{
    public string? Q { get; set; }
    public string? Community { get; set; }
    public string? Campus { get; set; }
    public string? Range { get; set; }
    public string? Sort { get; set; }
    public string? Status { get; set; }
    public string? View { get; set; }
}
