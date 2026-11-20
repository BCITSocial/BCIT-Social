
namespace BCIT_Social.Models;

// Represents a direct-message or group chat, including its details, members, messages, and membership history.
public class Chatbox
{
    // Unique identifier for the chatbox.
    public int Id { get; set; }

    // Chat type, such as DM or Group.
    // DM and Group would be the two values, default to DM. 
    public string ChatType { get; set; } = "DM";

    // Optional name for a group conversation.
    // Group chat name/Convo name. 
    public string? Name { get; set; }

    // Optional description for the chatbox.
    public string? Description { get; set; }

    // User who created the chatbox; null if user is permanently deleted.
    public string? CreatorId { get; set; }

    // Records when the chatbox was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    // User who created the chatbox; null if the user was deleted.
    public ApplicationUser? Creator { get; set; }

    // Messages sent in the chatbox.
    public ICollection<Message> Messages { get; set; }
        = new List<Message>();

    // Users who are currently members of the chatbox.
    public ICollection<ChatboxUser> ChatboxUsers { get; set; }
        = new List<ChatboxUser>();

    // Historical membership records for the chatbox.
    public ICollection<ChatboxMembershipHistory> MembershipHistory { get; set; }
        = new List<ChatboxMembershipHistory>();
}