// author: Andre Di Lascio
// Defines the presentation model for the Home feed and sidebar content.

using BCIT_Social.ViewModels;

namespace BCIT_Social.Models;

// Provides the data displayed on the home page.
public class HomeViewModel
{
    // The first page of the feed, newest first.
    public IReadOnlyList<PostCardViewModel> Posts { get; init; } = [];
    // True when older posts exist past the last card.
    public bool HasMore { get; init; }
    // The active feed filter, one of FeedFilters.
    public string Filter { get; init; } = FeedFilters.ALL;
    // The campus shown when Filter is campus. Null otherwise.
    public string? Campus { get; init; }
    // Every campus on a profile, for the Campus filter menu.
    public IReadOnlyList<string> Campuses { get; init; } = [];
    // Initials for the signed-in user's avatar in the share box. Empty when signed out.
    public string CurrentUserInitials { get; init; } = "";
}
