
namespace BCIT_Social.Models;

// Represents a direct-message or group chat, including its details, members, messages, and membership history.
public class Chatbox
{
    public int Id { get; set; }

    // DM and Group would be the two values, default to DM. 
    public string ChatType { get; set; } = "DM";

    // Group chat name/Convo name. 
    public string? Name { get; set; }

    // Chat description. 
    public string? Description { get; set; }

    public string CreatorId { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser Creator { get; set; } = null!;

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();

    public ICollection<ChatboxUser> ChatboxUsers { get; set; }
        = new List<ChatboxUser>();

    public ICollection<ChatboxMembershipHistory> MembershipHistory { get; set; }
    = new List<ChatboxMembershipHistory>();
}