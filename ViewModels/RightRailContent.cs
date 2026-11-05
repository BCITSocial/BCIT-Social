namespace BCIT_Social.ViewModels;

// The whole wwwroot/content/right-rail-text.json file: the rail text and the entries for each card.
public class RightRailContent
{
    public RightRailText Text { get; init; } = new();
    public IReadOnlyList<RailEventViewModel> Events { get; init; } = [];
    public IReadOnlyList<RailLinkViewModel> Links { get; init; } = [];
}
