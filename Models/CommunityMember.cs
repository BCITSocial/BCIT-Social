using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Models
{
    // Represents a user's membership and moderator status in a community.
    [PrimaryKey(nameof(UserId), nameof(CommunityId))]
    public class CommunityMember
    {
        // User identifier; part of the composite primary key.
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;

        // Community identifier; part of the composite primary key.
        [ForeignKey(nameof(Community))]
        public int CommunityId { get; set; }

        // Indicates whether the member can moderate the community.
        public bool IsModerator { get; set; }

        // Records when the user joined the community.
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        // User associated with this membership.
        public virtual ApplicationUser User { get; set; } = null!;

        // Community associated with this membership.
        public virtual Community Community { get; set; } = null!;
    }
}