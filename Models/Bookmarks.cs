using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Bookmarks
{
    public string User_Id { get; set; } = null!;

    public int Post_Id { get; set; }

    public DateTime Saved_At { get; set; } = DateTime.UtcNow;
}