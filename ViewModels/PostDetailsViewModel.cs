namespace BCIT_Social.ViewModels;

// Data for the post details modal and page: the post plus its comment threads.
public class PostDetailsViewModel
{
    public PostCardViewModel Post { get; init; } = new();
    // Top-level comments, oldest first. Replies are nested inside each comment.
    public IReadOnlyList<CommentViewModel> Comments { get; init; } = [];
    // Initials for the signed-in user's avatar beside the comment box. Empty when signed out.
    public string CurrentUserInitials { get; init; } = "";
}

// One comment and its replies.
public class CommentViewModel
{
    public int Id { get; init; }
    public string AuthorName { get; init; } = "";
    // True when the comment was written by the post's author; shows an "Author" badge.
    public bool IsPostAuthor { get; init; }
    public string Content { get; init; } = "";
    // Stored in UTC.
    public DateTime CreatedAt { get; init; }
    public List<CommentViewModel> Replies { get; } = [];

    public string AuthorInitials => DisplayFormat.Initials(AuthorName);

    // Total replies at every depth, for the "View N replies" toggle.
    public int ReplyCount => Replies.Count + Replies.Sum(reply => reply.ReplyCount);
}
