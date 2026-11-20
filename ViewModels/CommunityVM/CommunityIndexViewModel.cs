namespace BCIT_Social.ViewModels;

// Everything the Communities page. It shows the page text, community cards,
// and the popup form.
public class CommunityIndexViewModel
{
    public CommunitiesText Text { get; init; } = new();
    public IReadOnlyList<CommunityCardViewModel> Communities { get; init; } = [];
    public CommunityFormViewModel CreateForm { get; init; } = new();
    // False hides the "Your communities" section.
    public bool IsSignedIn { get; init; }

    // Joined communities show first, the rest are under "Browse communities".
    public IReadOnlyList<CommunityCardViewModel> YourCommunities => Communities.Where(card => card.IsJoined).ToList();
    public IReadOnlyList<CommunityCardViewModel> BrowseCommunities => Communities.Where(card => !card.IsJoined).ToList();
}

// Community card on the Communities page.
public class CommunityCardViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    // The community's description, or nothing when none.
    public string Description { get; init; } = "";
    public IReadOnlyList<CommunityTagBadge> Badges { get; init; } = [];
    // The campus tag, or "campus" when none.
    public string Location { get; init; } = "";
    public int MemberCount { get; init; }
    // True when the signed-in user is a member.
    public bool IsJoined { get; init; }
    // True when the signed-in user created it (can't leave, can open Settings).
    public bool IsCreator { get; init; }
    // Cover image URL, or null when none was uploaded.
    public string? BannerUrl { get; init; }

    // Values the filter dropdowns.
    // If more then one topic, joined with "|".
    public IReadOnlyList<string> Topics { get; init; } = [];
    public string? Status { get; init; }
    public string TopicsAttribute => string.Join("|", Topics);

    // Letters for the card's icon tile.
    public string Initials => DisplayFormat.Initials(Name);
}
