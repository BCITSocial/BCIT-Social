using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents a user's saved post.
public class Bookmarks
{
    // User identifier; part of the composite primary key.
    public string UserId { get; set; } = null!;

    // User who saved the post.
    public ApplicationUser User { get; set; } = null!;

    // Post identifier; part of the composite primary key.
    public int PostId { get; set; }

    // Post saved by the user.
    public Post Post { get; set; } = null!;

    // Records when the post was saved.
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}