using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Tin Trinh
/// Last Revised: Nov 20, 2026
/// View model of Admin Page
/// </summary>
public class AdminViewModel
{
    public List<Community> Communities { get; set; } = [];
    public List<ApplicationUser> Users { get; set; } = [];
}
