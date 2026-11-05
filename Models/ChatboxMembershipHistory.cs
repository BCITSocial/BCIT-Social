namespace BCIT_Social.Models;

// Records a user's join and leave history for a chatbox.
public class ChatboxMembershipHistory
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public int ChatboxId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Chatbox Chatbox { get; set; } = null!;
}