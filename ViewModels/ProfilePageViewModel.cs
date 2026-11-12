using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

/// <summary>
/// Author: Jonathan Yeh
/// Last Revised: Nov 11, 2026
/// View model for the profile page, including the user's profile, posts, bookmarks, and communities.
/// </summary>
public class ProfilePageViewModel
{
    public Profile? Profile { get; init; }
    public bool IsOwnProfile { get; init; }
    public IReadOnlyList<PostCardViewModel> Posts { get; init; } = [];
    public IReadOnlyList<PostCardViewModel> Bookmarks { get; init; } = [];
    public IReadOnlyList<CommunityLinkViewModel> Communities { get; init; } = [];
}
