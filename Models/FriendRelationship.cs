namespace BCIT_Social.Models;

// Represents a friendship or friend request between two users.
public class FriendRelationship
{
    // Unique identifier for the relationship.
    public int Id { get; set; }

    // Unique key representing the two users in this relationship.
    public string RelationshipKey { get; set; } = null!;

    // User who sent the friend request.
    public string RequesterId { get; set; } = null!;

    // User who received the friend request.
    public string RecipientId { get; set; } = null!;

    // Current relationship status, defaulting to Pending.
    public string Status { get; set; } = "Pending";

    // Records when the relationship was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Records when the relationship was last updated; null if it has not been updated.
    public DateTime? UpdatedAt { get; set; }

    // Records when the relationship was deactivated; null while active.
    public DateTime? DeactivatedAt { get; set; }

    // User who requested the friendship.
    public ApplicationUser Requester { get; set; } = null!;

    // User who received the request.
    public ApplicationUser Recipient { get; set; } = null!;
}