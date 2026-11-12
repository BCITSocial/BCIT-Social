using System.ComponentModel.DataAnnotations;

namespace BCIT_Social.Models;

// Represents a user-created post within the application.
public class Post
{
    // Unique identifier for the post.
    public int Id { get; set; }

    // User who created the post.
    public string AuthorId { get; set; } = null!;

    // Author of the post.
    public ApplicationUser Author { get; set; } = null!;

    // Optional community associated with the post.
    public int? CommunityId { get; set; }

    // Community associated with the post; null if there is no community.
    public Community? Community { get; set; }

    // Post title.
    public string Title { get; set; } = null!;

    // Tags associated with the post.
    public ICollection<PostTags> PostTags { get; set; } = new List<PostTags>();

    // Text content of the post.
    public string Content { get; set; } = null!;

    // Records when the post was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null when active; stores when the post was soft-deleted.
    public DateTime? DateOfDel { get; set; }

    // Photos attached to the post.
    public ICollection<PostPhoto> Photos { get; set; } = new List<PostPhoto>();
}