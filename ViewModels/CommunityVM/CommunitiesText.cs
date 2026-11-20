namespace BCIT_Social.ViewModels;

// Every piece of text on the Communities pages.
// Read from communities-text.json by CommunityController.
public class CommunitiesText
{
    // Page header.
    public string PageTitle { get; init; } = "Communities";
    public string PageSubtitle { get; init; } = "Join any of the communities listed below.";
    public string CreateButton { get; init; } = "+ Create";
    public string AlertMessage { get; set; } = "You have created the maximum amount of communities (3)!";

    // "Your communities" section (joined communities, shown first).
    public string YourHeading { get; init; } = "Your communities";
    public string YourEmpty { get; init; } = "You haven't joined any communities yet.";

    // The list and its cards.
    public string BrowseHeading { get; init; } = "Browse communities";
    // Shown when every community is already joined (or none exist).
    public string BrowseNone { get; init; } = "No other communities to browse right now.";
    // Browse shows a few cards, "See all" pages through the rest.
    public string SeeAllButton { get; init; } = "See all";
    public string ShowLessButton { get; init; } = "Show less";
    public int BrowsePreviewCount { get; init; } = 3;
    public int BrowsePageSize { get; init; } = 8;
    public CommunityPagerText Pager { get; init; } = new();
    // Shown when a community has no description.
    public string DefaultDescription { get; init; } = "No description yet.";
    // Shown as the location when a community has no campus tag.
    public string DefaultCampus { get; init; } = "Burnaby Campus";
    public string MembersLabel { get; init; } = "members";
    public string JoinButton { get; init; } = "Join";
    public string JoinedLabel { get; init; } = "Joined";

    // "Joined" dropdown on a card, and the leave confirmation.
    public CommunityMenuText Menu { get; init; } = new();
    public CommunityConfirmText LeaveConfirm { get; init; } = new();

    // Search box and filter dropdowns above the list.
    public CommunityFiltersText Filters { get; init; } = new();

    // Choices for the form dropdowns and the filters.
    public CommunityOptions Options { get; init; } = new();

    // Labels for the create form (pop-up creaete modal) edit form.
    public CommunityFormText CreateForm { get; init; } = new();
    public CommunityFormText EditForm { get; init; } = new()
    {
        Title = "Edit community",
        Subtitle = "Update community details, category, and tags.",
        NamePlaceholder = "",
        PreviewLabel = "Current tags preview",
        SubmitButton = "Save changes",
    };

    public CommunityDeleteText DeletePage { get; init; } = new();
}

// Pagination under "Browse communities" once "See all" is pressed.
public class CommunityPagerText
{
    public string Label { get; init; } = "Browse pages";
    public string Previous { get; init; } = "Prev";
    public string Next { get; init; } = "Next";
    public string End { get; init; } = "End";
}

// The "Joined" dropdown menu (cards and Details page).
public class CommunityMenuText
{
    public string Label { get; init; } = "Community options";
    public string View { get; init; } = "View community";
    public string Settings { get; init; } = "Settings";
    public string Leave { get; init; } = "Leave community";
}

// An "Are you sure?" popup. {name} in Message is replaced with the community name.
public class CommunityConfirmText
{
    public string Title { get; init; } = "Are you sure?";
    public string Message { get; init; } = "";
    public string ConfirmButton { get; init; } = "Confirm";
    public string CancelButton { get; init; } = "Cancel";

    public string MessageFor(string communityName) => Message.Replace("{name}", communityName);
}

// Search, filter dropdowns, and the message shown when nothing matches.
public class CommunityFiltersText
{
    // Labels for search and dropdowns.
    public string Label { get; init; } = "Filter communities";
    public string SearchLabel { get; init; } = "Search communities";
    public string SearchPlaceholder { get; init; } = "Search communities...";
    public string TypeLabel { get; init; } = "Topic";
    public string CampusLabel { get; init; } = "Campus";
    // Placeholder options of each dropdown.
    public string AllTypes { get; init; } = "All topics";
    public string AllCampuses { get; init; } = "All campuses";
    // Group names inside the topic dropdown.
    public string CategoriesGroup { get; init; } = "Categories";
    public string TagsGroup { get; init; } = "Tags";
    public string ClearButton { get; init; } = "Clear";
    public string EmptyMessage { get; init; } = "No communities match your search.";
}

// The tag choices.
public class CommunityOptions
{
    // First option of the Tag dropdown, meaning no value.
    public string NoneLabel { get; init; } = "None";
    // First option of the Campus dropdown, meaning no particular campus.
    public string AnyCampusLabel { get; init; } = "All campuses";

    // Broad groups (Academic, Club, ...).
    public IReadOnlyList<string> Categories { get; init; } = [];
    // Specific topics (Web Development, Hiking, ...).
    public IReadOnlyList<string> Tags { get; init; } = [];
    // No longer picked on the form. Kept so older communities
    // that saved a status or time tag still show it in the right color.
    public IReadOnlyList<string> Statuses { get; init; } = [];
    public IReadOnlyList<string> Times { get; init; } = [];
    public IReadOnlyList<string> Campuses { get; init; } = [];

    // Categories and tags together, without repeats.
    public IEnumerable<string> Topics => Categories.Concat(Tags).Distinct(StringComparer.OrdinalIgnoreCase);

    // True for "None" and "All campuses";
    public bool IsNoValue(string value) =>
        string.Equals(value, NoneLabel, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, AnyCampusLabel, StringComparison.OrdinalIgnoreCase);
}

// Labels for the community form.
public class CommunityFormText
{
    public string Title { get; init; } = "Create community";
    public string Subtitle { get; init; } = "Build a space around a shared interest.";
    // Cover image picker. Leave CoverTitle empty to hide it.
    public string CoverTitle { get; init; } = "";
    public string CoverSubtitle { get; init; } = "";
    public string CoverChange { get; init; } = "Change cover";
    public string CoverTooLarge { get; init; } = "Cover images must be {size} or smaller.";
    public string CoverInvalid { get; init; } = "Cover images must be a JPG, PNG, WEBP or GIF.";
    public string NameLabel { get; init; } = "Community name";
    public string NamePlaceholder { get; init; } = "";
    public string DescriptionLabel { get; init; } = "Description";
    public string DescriptionPlaceholder { get; init; } = "";
    public string CategoryLabel { get; init; } = "Category";
    public string CategoryHint { get; init; } = "";
    public string CampusLabel { get; init; } = "Campus";
    public string TagLabel { get; init; } = "Tag";
    public string TagHint { get; init; } = "";
    public string PreviewLabel { get; init; } = "Tags preview";
    public string HelperNote { get; init; } = "";
    public string CancelButton { get; init; } = "Cancel";
    public string SubmitButton { get; init; } = "Create community";
}

// The Delete community page.
public class CommunityDeleteText
{
    public string Title { get; init; } = "Delete community";
    public string Subtitle { get; init; } = "This action cannot be undone.";
    public string WarningPrefix { get; init; } = "Are you sure you want to permanently remove ";
    public string WarningSuffix { get; init; } = "? All associations and memberships will be lost.";
    public string ConfirmButton { get; init; } = "Delete community";
    public string CancelButton { get; init; } = "Cancel";
}