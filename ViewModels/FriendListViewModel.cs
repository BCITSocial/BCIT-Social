namespace BCIT_Social.ViewModels;

public class FriendListItemViewModel
{
    public int RelationshipId { get; init; }
    public string UserId { get; init; } = "";
    public string DisplayName { get; init; } = "";
}
