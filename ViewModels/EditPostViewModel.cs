using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Description: Contains the user-editable data for an existing post.
/// Author: Livjot Mahendru
/// </summary>
public class EditPostViewModel
{
	public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = null!;

    [Display(Name = "Post Type")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a post type.")]
	public int TagId { get; set; }

	[Required]
	public string Content { get; set; } = null!;

	public List<SelectListItem> PostTypes { get; set; } = new();
}