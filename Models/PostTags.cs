using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class PostTags
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public int TagId { get; set; }

    public Tags Tag { get; set; } = null!;

}