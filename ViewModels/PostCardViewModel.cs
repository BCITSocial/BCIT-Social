namespace BCIT_Social.ViewModels;

// Data for one post card in the Home feed.
public class PostCardViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
    // Profile display name, or the account user name when there is no profile.
    public string AuthorName { get; init; } = "";
    // Null when the post is not in a community.
    public string? CommunityName { get; init; }
    // Stored in UTC.
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public int CommentCount { get; init; }
    // True when the signed-in user has bookmarked the post.
    public bool IsSaved { get; init; }
    // True when the signed-in user authored the post.
    public bool IsAuthor { get; init; }

    public string AuthorInitials => DisplayFormat.Initials(AuthorName);

    public string TimeAgo(DateTime utcNow) => DisplayFormat.TimeAgo(CreatedAt, utcNow);
}
