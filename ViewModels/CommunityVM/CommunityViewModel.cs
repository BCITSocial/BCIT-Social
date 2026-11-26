using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Tin Trinh
/// Last Revised: Oct 13, 2026
/// Description: Holds parameters for user facing inputs when creating a new community.
/// </summary>
public class CommunityViewModel
{
    public int Id { get; set; }
    // Community name limit shared by the create and edit forms.
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [Required]
    public string Type { get; set; } = null!;
    public SelectList Types { get; set; }

    // Author: Jaspreet Bath
    // Ensure's modal description doesn't delete itself.
    // Limit the length of the optional community description.
    [StringLength(1000)]
    public string? Description { get; set; }
    // Cover image picked in the create pop-up or Settings. Null keeps the current one.
    public IFormFile? BannerImage { get; set; }
    // ----------------------
    public bool IsJoined { get; set; }
    public int MembersCount { get; set; }
    public IFormFile? BannerPhoto { get; set; }

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
