using System.ComponentModel.DataAnnotations;

namespace MvpDatabase.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual Profile Profile { get; set; }
        public virtual ICollection<Community> CreatedCommunities { get; set; }
        public virtual ICollection<CommunityMember> CommunityMemberships { get; set; }
    }
}