namespace BCIT_Social.ViewModels;

// Names of the card slots a page can put in the right rail.
// A page lists them in ViewData["RightRail"], for example [RightRailCards.Events, RightRailCards.Links].
public static class RightRailCards
{
    public const string Events = "events";
    public const string Links = "links";
}

// Data for the right rail: which card slots to show (in order), all wording, and the entries for each card.
public class RightRailViewModel
{
    public IReadOnlyList<string> Cards { get; init; } = [];
    public RightRailText Text { get; init; } = new();
    // The entries for each card. Backend fills these in.
    public IReadOnlyList<RailEventViewModel> Events { get; init; } = [];
    public IReadOnlyList<RailLinkViewModel> Links { get; init; } = [];
}
