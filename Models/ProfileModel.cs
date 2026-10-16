using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BCIT_Social.Models
{
    public class Profile
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        
        public string DisplayName { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public string Program { get; set; } = null!;
        public string Campus { get; set; } = null!;
        
        public virtual ApplicationUser User { get; set; } = null!;

        public string? ProfilePhotoStorageKey { get; set; }

        public string? ProfilePhotoOriginalFileName { get; set; }

        public string? ProfilePhotoContentType { get; set; }

        public long? ProfilePhotoFileSizeBytes { get; set; }

        public DateTime? ProfilePhotoUpdatedAt { get; set; }
    }
}