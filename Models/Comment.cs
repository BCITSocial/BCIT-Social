using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Comments
{
    public int Id { get; set; }

    public string AuthorId { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? ParentCommentId { get; set; } // Nullable for top-level comments

    public Comments? ParentComment { get; set; }

    public ICollection<Comments> Replies { get; set; } = new List<Comments>();
}