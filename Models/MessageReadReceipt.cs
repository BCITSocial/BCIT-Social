namespace BCIT_Social.Models;

// Records when a user has read a specific message.
public class MessageReadReceipt
{
    // Message that was read; part of the composite primary key.
    public int MessageId { get; set; }

    // User who read the message; part of the composite primary key.
    public string UserId { get; set; } = null!;

    // Records when the message was read.
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Message associated with this read receipt.
    public Message Message { get; set; } = null!;

    // User associated with this read receipt.
    public ApplicationUser User { get; set; } = null!;
}