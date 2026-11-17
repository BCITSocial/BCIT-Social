namespace BCIT_Social.ViewModels;

// The whole wwwroot/content/events-text.json file: the page text, the browsing tags and the event cards.
public class EventsContent
{
    public EventsText Text { get; init; } = new();
    // Tags shown as chips above the cards, e.g. "Workshop". Empty means no tag row.
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<EventCardViewModel> Events { get; init; } = [];
}
