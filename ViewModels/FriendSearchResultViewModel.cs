namespace BCIT_Social.ViewModels;

public class FriendSearchResultViewModel
{
    public string UserId { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Program { get; init; } = "";
    public string Campus { get; init; } = "";
    public string Email { get; init; } = "";
    public string? RelationshipStatus { get; init; }

    /// <summary>Describes what the viewer can do with a search result.</summary>
    public static string GetRelationshipStatus(string? status, string? requesterId, string viewerId)
    {
        if (status == "Accepted") return "Friends";
        if (status != "Pending") return "None";
        return requesterId == viewerId ? "Sent" : "Received";
    }
}
