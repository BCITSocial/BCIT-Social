using System.ComponentModel.DataAnnotations.Schema;

namespace MvpDatabase.Models
{
    public class CommunityMember
    {
        [ForeignKey("User")]
        public string UserId { get; set; }
        
        [ForeignKey("Community")]
        public int CommunityId { get; set; }
        
        public int Mod { get; set; } // 0 or 1 based on the diagram
        
        public DateTime JoinedAt { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Community Community { get; set; }
    }
}