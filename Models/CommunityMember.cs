using System.ComponentModel.DataAnnotations.Schema;

namespace BCIT_Social.Models
{
    public class CommunityMember
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        
        [ForeignKey(nameof(Community))]
        public int CommunityId { get; set; }
        
        public bool IsModerator { get; set; } 
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Community Community { get; set; } = null!;
    }
}