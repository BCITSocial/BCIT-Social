using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BCIT_Social.Models
{
    public class Community
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey(nameof(Creator))]
        public string CreatorId { get; set; } = null!;
        
        [Required]
        public string Name { get; set; } = null!;
        
        public string Type { get; set; } = null!;
        
        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();
    }
}