using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Post
{
    public int Id { get; set; }

    public string Author_id { get; set; }

    public int Community_Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime Date_Of_Del { get; set; }


}