using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Models;

[PrimaryKey(nameof(EventId), nameof(UserId))]
public class EventRsvp
{
    public int EventId { get; set; }
    public string UserId { get; set; } = null!;
    public bool Going { get; set; }
    public CommunityEvent Event { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
