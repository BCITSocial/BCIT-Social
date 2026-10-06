using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvpDatabase.Models
{
    public class Community
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey("Creator")]
        public string CreatorId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Type { get; set; } 
        
        public virtual User Creator { get; set; }
        public virtual ICollection<CommunityMember> Members { get; set; }
    }
}