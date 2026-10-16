using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Bookmarks
{
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}