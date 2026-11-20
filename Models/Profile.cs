using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BCIT_Social.Models
{
    // Stores profile information associated with a user account.
    public class Profile
    {
        // User identifier and primary key for the profile.
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        
        // User's display name.
        public string DisplayName { get; set; } = null!;

        // User's profile biography.
        public string Bio { get; set; } = null!;

        // User's academic program.
        public string Program { get; set; } = null!;

        // User's campus.
        public string Campus { get; set; } = null!;
        
        // User associated with this profile.
        public virtual ApplicationUser User { get; set; } = null!;

        // Storage key for the profile photo.
        public string? ProfilePhotoStorageKey { get; set; }

        // Original filename of the profile photo.
        public string? ProfilePhotoOriginalFileName { get; set; }

        // MIME type of the profile photo.
        public string? ProfilePhotoContentType { get; set; }

        // Size of the profile photo in bytes.
        public long? ProfilePhotoFileSizeBytes { get; set; }

        // Records when the profile photo was last updated.
        public DateTime? ProfilePhotoUpdatedAt { get; set; }
    }
}