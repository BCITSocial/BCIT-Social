using System;
using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents an event associated with a community.
public class CommunityEvent
{
    // Unique identifier for the event.
    public int Id { get; set; }

    // User who created the event; null if user is permanently deleted.
    public string? CreatorId { get; set; }

    // Associated community; null if the community was permanently deleted.
    public int? CommunityId { get; set; }

    // Event title.
    public string Title { get; set; } = null!;

    // Date and time when the event occurs.
    public DateTime EventDate { get; set; }

    // Null when active; stores when the event is soft-deleted.
    public DateTime? DateOfDel { get; set; }

    // User who created the event; null if the user is deleted.
    public ApplicationUser? Creator { get; set; }

    // Community associated with the event; null if unavailable.
    public Community? Community { get; set; }
}