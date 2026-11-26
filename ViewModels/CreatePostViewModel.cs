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

    [Display(Name = "Post Type")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a post type.")]
	public int TagId { get; set; }

	public List<SelectListItem> PostTypes { get; set; } = new();

    // Optional community for the post and the available dropdown choices.
    public int? CommunityId { get; set; }
    public List<SelectListItem> Communities { get; set; } = new();

	[Required]
	// Matches the content limit in the create post form.
	[StringLength(5000)]
	public string Content { get; set; } = null!;
	
	[Display(Name = "Photos")]
	public List<IFormFile> Photos { get; set; } = new();
	}
