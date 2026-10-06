using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Message
{
    public int Id { get; set; }

    public int ChatboxId { get; set; }

    public string SenderId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Chatbox Chatbox { get; set; } = null!;

    public IdentityUser Sender { get; set; } = null!;
}