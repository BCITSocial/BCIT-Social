using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Tin Trinh
/// Last Revised: Oct 13, 2026
/// Description: Holds parameters for user facing inputs when creating a new community.
/// </summary>
public class CommunityViewModel
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Type { get; set; } = null!;
    public SelectList Types { get; set; }

    public CommunityViewModel()
    {
        List<SelectListItem> typesList =
        [
            new SelectListItem { Value = "Club", Text = "Club"},
            new SelectListItem { Value = "Intake", Text = "Intake"},
            new SelectListItem { Value = "Program", Text = "Program"},
            new SelectListItem { Value = "Other", Text = "Other"},
        ];
        Types = new(typesList, "Value", "Text");
    }

}
