using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Comments
{
    public int Id { get; set; }

    public string Author_id { get; set; }

    public int Post_Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? ParentCommentId { get; set; } // Nullable for top-level comments
}