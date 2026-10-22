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

        public string? BannerPhotoStorageKey { get; set; }

        public string? BannerPhotoOriginalFileName { get; set; }

        public string? BannerPhotoContentType { get; set; }

        public long? BannerPhotoFileSizeBytes { get; set; }

        public DateTime? BannerPhotoUpdatedAt { get; set; }

        // Author: Jaspreet Bath
        // Ensure's modal description doesn't delete itself.
        public string? Description { get; set; }
    }
}