
namespace BCIT_Social.Models;

// Represents a message sent within a chatbox, including its sender, timestamp, and read receipts.
public class Message
{
    // Unique identifier for the message.
    public int Id { get; set; }

    // Chatbox containing the message.
    public int ChatboxId { get; set; }

    // User who sent the message; null if user is permanently deleted.
    public string? SenderId { get; set; }

    // Text content of the message.
    public string Text { get; set; } = null!;

    // Records when the message was sent.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Chatbox containing this message.
    public Chatbox Chatbox { get; set; } = null!;

    // User who sent the message; null if the user was deleted.
    public ApplicationUser? Sender { get; set; }

    // Users who have read the message and when they read it.
    public ICollection<MessageReadReceipt> ReadReceipts { get; set; }
    = new List<MessageReadReceipt>();
}