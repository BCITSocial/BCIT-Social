namespace BCIT_Social.ViewModels;

// Deleting a community confirmation page.
public class CommunityDeleteViewModel
{
    public CommunityDeleteText Text { get; init; } = new();
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public IReadOnlyList<CommunityTagBadge> Badges { get; init; } = [];
    public string Location { get; init; } = "";
    public int MemberCount { get; init; }
    public string MembersLabel { get; init; } = "members";
}