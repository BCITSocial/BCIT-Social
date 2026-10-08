using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class ChatboxUser
{
    public string UserId { get; set; } = null!;

    public int ChatboxId { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;

    public Chatbox Chatbox { get; set; } = null!;
}