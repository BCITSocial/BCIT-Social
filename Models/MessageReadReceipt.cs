namespace BCIT_Social.Models;

// Records when a user has read a specific message.
public class MessageReadReceipt
{
    public int MessageId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public Message Message { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}