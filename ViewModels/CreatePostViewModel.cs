using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Description: Contains the user-entered data required to create a post. 
/// This way the browser can only submit title, tags, posttype, & content.
/// Author: Livjot M
/// </summary>
public class CreatePostViewModel
{
	[Required]
	[StringLength(150)]
	public string Title { get; set; } = null!;

    [Display(Name = "Tags")]
	[MinLength(1, ErrorMessage = "Choose at least one tag.")]
	[MaxLength(3, ErrorMessage = "Choose up to 3 tags.")]
	public List<int> TagIds { get; set; } = new();

	public List<SelectListItem> PostTypes { get; set; } = new();

	[Required]
	public string Content { get; set; } = null!;
	
	// Set when the post is made from inside a community; null posts to the main feed.
	public int? CommunityId { get; set; }

	// Shown in the create post popup when CommunityId is set. Never read from the form.
	[Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
	public string? CommunityName { get; set; }

	[Display(Name = "Photos")]
	public List<IFormFile> Photos { get; set; } = new();
	}