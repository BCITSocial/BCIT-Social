namespace BCIT_Social.Models;
public class FriendRelationship
{
    public int Id { get; set; }

    public string RelationshipKey { get; set; } = null!;

    public string RequesterId { get; set; } = null!;
    public string RecipientId { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeactivatedAt { get; set; }

    public ApplicationUser Requester { get; set; } = null!;
    public ApplicationUser Recipient { get; set; } = null!;
}