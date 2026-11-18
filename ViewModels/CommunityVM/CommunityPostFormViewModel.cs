using System.ComponentModel.DataAnnotations;

namespace BCIT_Social.ViewModels;

// What the create post pop-up on a community's page submits to 
// CommunityController.CreatePost.
public class CommunityPostFormViewModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    // Post type (a Tags row). Optional so posting still works before any post types exist.
    public int? TagId { get; set; }
}
