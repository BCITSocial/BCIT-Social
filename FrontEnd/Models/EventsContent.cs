namespace BCITSocial.FrontEnd.Models;

// The shape of wwwroot/content/events-text.json. One entry in "events" makes one card.
public class EventsContent
{
    public IReadOnlyList<EventCardViewModel> Events { get; init; } = [];
}
