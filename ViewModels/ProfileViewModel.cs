using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BCIT_Social.ViewModels;

/// </summary>
/// Author: Joshua Trepanier
/// Last Revised: Oct 15, 2026
/// Description: View model for creating Profile model (The profile itself)
/// </summary>
public class ProfileViewModel
{

    [Required]
    public string DisplayName { get; set; } = null!;

    public string Bio { get; set; } = null!;


    public string Program { get; set; } = null!;

    public string Campus { get; set; } = null!;
    // Storage key for the profile photo.
    public string? ProfilePhotoStorageKey { get; set; }
    public string? ProfilePhotoOriginalFileName { get; set; }
    public string? ProfilePhotoContentType { get; set; }
    public long? ProfilePhotoFileSizeBytes { get; set; }
    public DateTime? ProfilePhotoUpdatedAt { get; set; }

    public IFormFile? ProfilePhoto { get; set; }

}
