namespace BCIT_Social.ViewModels;

// Every piece of text on the Community Details page, read from 
// community-details-text.json by CommunityController.
public class CommunityDetailsText
{
    public CommunityBannerText Banner { get; init; } = new();
    public CommunityMetaText Meta { get; init; } = new();
    public string AboutHeading { get; init; } = "About";
    // Tabs under the header.
    public IReadOnlyList<string> NavTabs { get; init; } = ["Overview"];
    public CommunityDiscussionsText Discussions { get; init; } = new();
}

// The banner at the top of the page.
public class CommunityBannerText
{
    public string Subtitle { get; init; } = "";
    public string Title { get; init; } = "";
    public string Caption { get; init; } = "";
    public string CodeBadge { get; init; } = "";
}

// Community name, tags, and Join / Leave / Create / Settings.
public class CommunityMetaText
{
    public string JoinedLabel { get; init; } = "Joined";
    public string JoinButton { get; init; } = "+ Join";
    public string LeaveButton { get; init; } = "Leave";
    public string Privacy { get; init; } = "Public community";
    // Shown as the location when the community has no campus tag.
    public string Campus { get; init; } = "Burnaby Campus";
    public string MembersLabel { get; init; } = "members";
    public string CreateButton { get; init; } = "+ Create";
    public string SettingsButton { get; init; } = "Settings";
}

// "Latest discussions" placeholders.
public class CommunityDiscussionsText
{
    public string Title { get; init; } = "Latest discussions";
    public string SeeAllText { get; init; } = "See all";
    public string ShowLessText { get; init; } = "Show less";
    // How many discussions show before "See all" is pressed.
    public int InitiallyShown { get; init; } = 2;
    public IReadOnlyList<CommunityDiscussionText> Items { get; init; } = [];
}

// Discussion card.
public class CommunityDiscussionText
{
    public string Initials { get; init; } = "";
    public string Title { get; init; } = "";
    public string Time { get; init; } = "";
    public string Body { get; init; } = "";
    public string AuthorMeta { get; init; } = "";
    public string RepliesCount { get; init; } = "";
}