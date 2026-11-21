using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Tin Trinh
/// Last Revised: Nov 20, 2026
/// View model of Admin Page
/// </summary>
public class AdminCommunityViewModel
{
    public string Name { get; set; } = null!;
    public DateTime? DateOfDel { get; set; }
}
