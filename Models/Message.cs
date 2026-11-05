
namespace BCIT_Social.Models;

// Represents a message sent within a chatbox, including its sender, timestamp, and read receipts.
public class Message
{
    public int Id { get; set; }

    public int ChatboxId { get; set; }

    public string SenderId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Chatbox Chatbox { get; set; } = null!;

    public ApplicationUser Sender { get; set; } = null!;

    public ICollection<MessageReadReceipt> ReadReceipts { get; set; }
    = new List<MessageReadReceipt>();
}