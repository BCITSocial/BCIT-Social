namespace BCIT_Social.ViewModels;

// The whole wwwroot/content/events-text.json file: the page text and the event cards.
public class EventsContent
{
    public EventsText Text { get; init; } = new();
    public IReadOnlyList<EventCardViewModel> Events { get; init; } = [];
}
