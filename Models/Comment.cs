using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents a comment or reply attached to a post.
public class Comments
{
    // Unique identifier for the comment.
    public int Id { get; set; }

    // User who wrote the comment; null if permanently deleted.
    public string? AuthorId { get; set; }

    // Author of the comment; null if the user was deleted.
    public ApplicationUser? Author { get; set; }

    // Post containing the comment.
    public int PostId { get; set; }

    // Post containing this comment.
    public Post Post { get; set; } = null!;

    // Text content of the comment.
    public string Content { get; set; } = null!;

    // Records when the comment was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional parent comment identifier; null for top-level comments.
    public int? ParentCommentId { get; set; } // Nullable for top-level comments

    // Parent comment for this reply; null for top-level comments.
    public Comments? ParentComment { get; set; }

    // Replies directly attached to this comment.
    public ICollection<Comments> Replies { get; set; } = new List<Comments>();
}