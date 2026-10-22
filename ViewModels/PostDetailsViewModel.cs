namespace BCIT_Social.ViewModels;

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
/// <summary>
/// Description: Contains the information displayed on the Post Details page.
/// Author: Livjot Mahendru
/// </summary>
public class PostDetailsViewModel
{
    public int Id { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? PostType { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool CanEdit { get; set; }
  
    // Data for the post details modal and page: the post plus its comment threads.
    public PostCardViewModel Post { get; init; } = new();
    // Top-level comments, oldest first. Replies are nested inside each comment.
    public IReadOnlyList<CommentViewModel> Comments { get; init; } = [];
    // Initials for the signed-in user's avatar beside the comment box. Empty when signed out.
    public string CurrentUserInitials { get; init; } = "";
}
