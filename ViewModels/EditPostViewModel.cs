using Microsoft.AspNetCore.Mvc.ModelBinding;
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

	[Display(Name = "Tags")]
	[MinLength(1, ErrorMessage = "Choose at least one tag.")]
	[MaxLength(3, ErrorMessage = "Choose up to 3 tags.")]
	public List<int> TagIds { get; set; } = new();

	[Required]
	public string Content { get; set; } = null!;

	public List<SelectListItem> PostTypes { get; set; } = new();

	// The page to go back to after saving or cancelling. The controller only keeps local URLs.
	public string ReturnUrl { get; set; } = "/";

	// Shown on the edit page for a community post. Never read from the form.
	[BindNever]
	public string? CommunityName { get; set; }
}
