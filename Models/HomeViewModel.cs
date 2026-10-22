// author: Andre Di Lascio
// Defines the presentation model for the Home feed and sidebar content.

namespace BCIT_Social.Models;

public class HomeViewModel
{
    public List<HomePostViewModel> Posts { get; set; } = new();
}

public class HomePostViewModel
{
    public int Id { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? PostType { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}