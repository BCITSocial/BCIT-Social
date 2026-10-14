using System.ComponentModel.DataAnnotations;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Description: Contains the user-entered data required to create a post. 
/// This way the browser can only submit title, tags, posttype, & content.
/// Author: Livjot M
/// </summary>
public class CreatePostViewModel
{
	//[Required]
	//[StringLength(150)]
	//public string Title { get; set; } = null!;

	[Required]
	public int TagId { get; set; }

	[Required]
	public string Content { get; set; } = null!;
}