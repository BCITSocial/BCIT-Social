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

    // Featured card at the
    public CommunityFeaturedText Featured { get; init; } = new();

    // The list and its cards.
    public string BrowseHeading { get; init; } = "Browse communities";
    // Shown when a community has no description.
    public string DefaultDescription { get; init; } = "No description yet.";
    // Shown as the location when a community has no campus tag.
    public string DefaultCampus { get; init; } = "Burnaby Campus";
    public string MembersLabel { get; init; } = "members";
    public string JoinButton { get; init; } = "Join";
    public string JoinedLabel { get; init; } = "Joined";

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

// The "This week" card on the Communities page.
public class CommunityFeaturedText
{
    public string Label { get; init; } = "This week";
    public string Heading { get; init; } = "";
    public string Description { get; init; } = "";
    public string Tag { get; init; } = "";
    public string StatusTag { get; init; } = "";
    public string TimeTag { get; init; } = "";
}

// Search, filter dropdowns, and the message shown when nothing matches.
public class CommunityFiltersText
{
    // Labels for search and dropdowns.
    public string Label { get; init; } = "Filter communities";
    public string SearchLabel { get; init; } = "Search communities";
    public string SearchPlaceholder { get; init; } = "Search communities...";
    public string TypeLabel { get; init; } = "Type";
    public string CampusLabel { get; init; } = "Campus";
    public string StatusLabel { get; init; } = "Status";
    // Placeholder options of each dropdown.
    public string AllTypes { get; init; } = "All types";
    public string AllCampuses { get; init; } = "All campuses";
    public string AnyStatus { get; init; } = "Any status";
    public string ClearButton { get; init; } = "Clear";
    public string EmptyMessage { get; init; } = "No communities match your search.";
}

// The tag choices.
public class CommunityOptions
{
    // First option of the Tag, Status and Time dropdowns, meaning no value.
    public string NoneLabel { get; init; } = "None";
    // First option of the Campus dropdown, meaning no particular campus.
    public string AnyCampusLabel { get; init; } = "All campuses";

    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
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
    public string CoverTitle { get; init; } = "";
    public string CoverSubtitle { get; init; } = "";
    public string NameLabel { get; init; } = "Community name";
    public string NamePlaceholder { get; init; } = "";
    public string DescriptionLabel { get; init; } = "Description";
    public string DescriptionPlaceholder { get; init; } = "";
    public string CategoryLabel { get; init; } = "Category";
    public string CampusLabel { get; init; } = "Campus";
    public string TagLabel { get; init; } = "Tag";
    public string StatusLabel { get; init; } = "Status tag";
    public string TimeLabel { get; init; } = "Time tag";
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