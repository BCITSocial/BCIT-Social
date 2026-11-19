namespace BCIT_Social.ViewModels;

public class FriendSearchResultViewModel
{
    public string UserId { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Program { get; init; } = "";
    public string Campus { get; init; } = "";
    public string? RelationshipStatus { get; init; }
}
