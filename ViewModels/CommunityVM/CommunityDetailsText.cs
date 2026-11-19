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
    public CommunityPostFormText PostForm { get; init; } = new();
    public CommunitySettingsText Settings { get; init; } = new();
    public CommunityConfirmText LeaveConfirm { get; init; } = new();
    public CommunityConfirmText DeleteConfirm { get; init; } = new();
}

// The banner at the top of the page.
public class CommunityBannerText
{
    public string Subtitle { get; init; } = "";
    public string Title { get; init; } = "";
    public string Caption { get; init; } = "";
    public string CodeBadge { get; init; } = "";
}

// Community name, tags, and Join / Members / Joined menu.
public class CommunityMetaText
{
    public string JoinedLabel { get; init; } = "Joined";
    public string JoinButton { get; init; } = "+ Join";
    public string LeaveButton { get; init; } = "Leave community";
    public string Privacy { get; init; } = "Community";
    // Shown as the location when the community has no campus tag.
    public string Campus { get; init; } = "Burnaby Campus";
    public string MembersLabel { get; init; } = "members";
    public string SettingsButton { get; init; } = "Settings";
    public string MenuLabel { get; init; } = "Community options";
}

// The "Posts" section.
public class CommunityDiscussionsText
{
    public string Title { get; init; } = "Posts";
    public string PostButton { get; init; } = "+ Post";
    // Shown instead of the Post button to visitors who haven't joined.
    public string JoinToPost { get; init; } = "Join this community to post.";
    public string EmptyText { get; init; } = "No posts yet.";
    public string PostedBy { get; init; } = "Posted by";
    public string CommentsLabel { get; init; } = "comments";
    public string SeeAllText { get; init; } = "See all";
    public string ShowLessText { get; init; } = "Show less";
    // How many posts show before "See all" is pressed.
    public int InitiallyShown { get; init; } = 3;
}

// The create post pop-up.
public class CommunityPostFormText
{
    public string Title { get; init; } = "Create post";
    // {name} is replaced with the community name.
    public string Subtitle { get; init; } = "Share something with {name}.";
    public string TypeLabel { get; init; } = "Post type";
    public string TitleLabel { get; init; } = "Title";
    public string TitlePlaceholder { get; init; } = "";
    public string ContentLabel { get; init; } = "Content";
    public string ContentPlaceholder { get; init; } = "";
    public string CancelButton { get; init; } = "Cancel";
    public string SubmitButton { get; init; } = "Publish post";
}

// The Settings pop-up. Its form labels come from the edit form in communities-text.json.
public class CommunitySettingsText
{
    public string Title { get; init; } = "Community settings";
    public string Subtitle { get; init; } = "Update details, tags, and the cover image.";
    public string DangerText { get; init; } = "Deleting removes the community and all memberships. This can't be undone.";
    public string DeleteButton { get; init; } = "Delete community";
}
