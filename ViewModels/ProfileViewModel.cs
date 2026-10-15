using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BCIT_Social.ViewModels;

/// <summary>
/// View model for creating Profile model (The profile itself)
/// </summary>
public class ProfileViewModel
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string DisplayName { get; set; } = null!;

    public string Bio { get; set; } = null!;

    public string Program { get; set; } = null!;

    public string Campus { get; set; } = null!;
}
