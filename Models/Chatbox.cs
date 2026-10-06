using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class Chatbox
{
    public int Id { get; set; }

    public string CreatorId { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public IdentityUser Creator { get; set; } = null!;

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();

    public ICollection<ChatboxUser> ChatboxUsers { get; set; }
        = new List<ChatboxUser>();
}