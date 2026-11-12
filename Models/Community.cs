using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BCIT_Social.Models
{
    // Represents a community that users can join and manage.
    public class Community
    {
        // Unique identifier for the community.
        [Key]
        public int Id { get; set; }

        // User who currently owns the community.
        [Required]
        [ForeignKey(nameof(Creator))]
        public string CreatorId { get; set; } = null!;

        // Community name.
        [Required]
        public string Name { get; set; } = null!;

        // Community category or type.
        public string Type { get; set; } = null!;

        // Current community owner.
        public virtual ApplicationUser Creator { get; set; } = null!;

        // Users who belong to the community.
        public virtual ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();

        // Storage key for the community banner image.
        public string? BannerPhotoStorageKey { get; set; }

        // Original filename of the banner image.
        public string? BannerPhotoOriginalFileName { get; set; }

        // MIME type of the banner image.
        public string? BannerPhotoContentType { get; set; }

        // Size of the banner image in bytes.
        public long? BannerPhotoFileSizeBytes { get; set; }

        // Records when the banner image was last updated.
        public DateTime? BannerPhotoUpdatedAt { get; set; }

        // Author: Jaspreet Bath
        // Ensure's modal description doesn't delete itself.
        // Optional community description.
        public string? Description { get; set; }

        // Null when active; stores DateTime when the community is soft-deleted.
        public DateTime? DateOfDel { get; set; }
    }
}