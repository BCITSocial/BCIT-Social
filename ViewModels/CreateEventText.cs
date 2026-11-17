namespace BCIT_Social.ViewModels;

// Every piece of text in the "Create event" pop-up on the Events page, read from "text" > "createEvent"
// in wwwroot/content/events-text.json. Each default is used when the JSON leaves that key out.
public class CreateEventText
{
    // Button next to the page heading that opens the pop-up.
    public string OpenButton { get; init; } = "Create event";

    public string Title { get; init; } = "Create event";
    public string Subtitle { get; init; } = "Share an event with your community.";
    public string Close { get; init; } = "Close";

    // Field labels, placeholders and help lines.
    public string LabelTitle { get; init; } = "Event title";
    public string PlaceholderTitle { get; init; } = "Resume workshop";
    public string LabelCommunity { get; init; } = "Community";
    public string ChooseCommunity { get; init; } = "Choose a community";
    public string NoCommunities { get; init; } = "Join a community to host events.";
    public string LabelStart { get; init; } = "Starts";
    public string LabelEnd { get; init; } = "Ends (optional)";
    public string LabelCampus { get; init; } = "Campus";
    public string ChooseCampus { get; init; } = "Choose a campus";
    public string LabelLocation { get; init; } = "Room or venue";
    public string PlaceholderLocation { get; init; } = "SE12";
    public string LabelDescription { get; init; } = "Description";
    public string PlaceholderDescription { get; init; } = "What should people know before they come?";
    public string LabelTags { get; init; } = "Tags (optional)";
    public string LabelCover { get; init; } = "Banner image (optional)";
    public string CoverHelp { get; init; } = "A wide image works best. It is cropped to 2:1.";

    // Error messages shown under a field when the form is sent with a problem.
    public string ErrorTitle { get; init; } = "Enter a title.";
    public string ErrorCommunity { get; init; } = "Choose a community.";
    public string ErrorStart { get; init; } = "Choose a start date and time.";
    public string ErrorStartPast { get; init; } = "The start time has already passed.";
    public string ErrorEnd { get; init; } = "The end time must be after the start time.";
    public string ErrorCampus { get; init; } = "Choose a campus.";
    // Shown at the top of the form when one or more fields have a problem.
    public string ErrorSummary { get; init; } = "Check the highlighted fields.";

    // Footer buttons. Submitting replaces the button text while the form is being sent.
    public string Cancel { get; init; } = "Cancel";
    public string Submit { get; init; } = "Create event";
    public string Submitting { get; init; } = "Creating...";

    // Shown in the form after a valid submit while saving is not connected yet.
    public string NotReady { get; init; } = "Creating events isn't available yet.";
    // Shown on the Events page after an event is created.
    public string Created { get; init; } = "Your event was created.";
}
