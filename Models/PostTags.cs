using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents the relationship between a post and a tag.
public class PostTags
{
    // Post identifier; part of the composite primary key.
    public int PostId { get; set; }

    // Post associated with the tag.
    public Post Post { get; set; } = null!;

    // Tag identifier; part of the composite primary key.
    public int TagId { get; set; }

    // Tag associated with the post.
    public Tags Tag { get; set; } = null!;

}