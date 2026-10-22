// author: Andre Di Lascio
// Defines the presentation model for the Home feed and sidebar content.

using BCIT_Social.ViewModels;

namespace BCIT_Social.Models;

public class HomeViewModel
{
    // The first page of the feed, newest first.
    public IReadOnlyList<PostCardViewModel> Posts { get; init; } = [];
    // True when older posts exist past the last card.
    public bool HasMore { get; init; }
    // Up to three communities the signed-in user has joined. Empty when signed out.
    public IReadOnlyList<CommunityLinkViewModel> YourCommunities { get; init; } = [];
    // Initials for the signed-in user's avatar in the share box. Empty when signed out.
    public string CurrentUserInitials { get; init; } = "";
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