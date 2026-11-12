namespace BCIT_Social.Models;

// Records a user's join and leave history for a chatbox.
public class ChatboxMembershipHistory
{
    // Unique identifier for the history record.
    public int Id { get; set; }

    // User identifier; null if the user was permanently deleted.
    public string? UserId { get; set; }

    // Chatbox associated with this history record.
    public int ChatboxId { get; set; }

    // Records when the user joined the chatbox.
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Records when the user left; null if no leave has been recorded.
    public DateTime? LeftAt { get; set; }

    // User associated with the history record; null if the user was deleted.
    public ApplicationUser? User { get; set; }

    // Chatbox associated with the history record.
    public Chatbox Chatbox { get; set; } = null!;
}