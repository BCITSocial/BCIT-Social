using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Tin Trinh
/// Last Revised: Nov 20, 2026
/// View model of Admin Page
/// </summary>
public class AdminUserViewModel
{
    public ApplicationUser AppUser { get; set; } = null!;
    public List<Community> CommunitiesOwned { get; set; } = [];
}
