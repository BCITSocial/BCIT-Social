using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Joshua Trepanier
/// Last Revised: Nov 19, 2026
/// Description: Page redirected to for managing all the users owned communities
/// </summary>
public class CommunityHandlingViewModel
{
    public IReadOnlyList<Community> Communities { get; init; } = [];

    public string? Id { get; set; }
}