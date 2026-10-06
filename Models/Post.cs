using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Post
{
    public int Id { get; set; }

    public string AuthorId { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;

    public int? CommunityId { get; set; }
    public Community? Community { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DateOfDel { get; set; }


}