namespace BCIT_Social.ViewModels;

public class IncomingFriendRequestViewModel
{
    public int Id { get; init; }
    public string RequesterId { get; init; } = "";
    public string RequesterDisplayName { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public bool HasPicture { get; init; }
}
