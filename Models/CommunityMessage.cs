using System;
using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents a message posted within a community.
public class CommunityMessage
{
    // Unique identifier for the message.
    public int Id { get; set; }

    // Community containing the message.
    public int CommunityId { get; set; }

    // User who sent the message; null if permanently deleted.
    public string? SenderId { get; set; }

    // Text content of the message.
    public string MessageBody { get; set; } = null!;

    // Records when the message was sent.
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Community containing this message.
    public Community Community { get; set; } = null!;

    // User who sent the message; null if the user was deleted.
    public ApplicationUser? Sender { get; set; }
}