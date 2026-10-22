namespace BCIT_Social.ViewModels;

/// <summary>
/// Description: Contains the information displayed on the Post Details page.
/// Author: Livjot Mahendru
/// </summary>
public class PostDetailsViewModel
{
    public int Id { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? PostType { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool CanEdit { get; set; }
}