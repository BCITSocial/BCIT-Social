using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents a tag that can be assigned to posts.
public class Tags
{
    // Unique identifier for the tag.
    public int Id { get; set; }

    // Tag name.
    public string Name { get; set; } = null!;

}