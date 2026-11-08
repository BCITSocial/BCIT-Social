using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents a user's current membership and permissions within a chatbox.
public class ChatboxUser
{
    // User identifier; part of the composite primary key.
    public string UserId { get; set; } = null!;

    // Chatbox identifier; part of the composite primary key.
    public int ChatboxId { get; set; }

    // Navigation properties

    // User associated with this chatbox membership.
    public ApplicationUser User { get; set; } = null!;

    // Chatbox associated with this membership.
    public Chatbox Chatbox { get; set; } = null!;

    // Indicates whether the user is an administrator of the chatbox.
    public bool IsAdmin { get; set; }
}