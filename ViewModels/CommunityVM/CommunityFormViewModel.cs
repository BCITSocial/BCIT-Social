namespace BCIT_Social.ViewModels;

// Community form: its labels, dropdown choices, and the values to show.
// Used by the create pop-up modal, Create and Edit page.
// All rendered to communites-form.json, read by CommunityController.
public class CommunityFormViewModel
{
    public CommunityFormText Text { get; init; } = new();
    public CommunityOptions Options { get; init; } = new();

    // Community being edited; null when creating.
    public int? Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    // The dropdowns' selected values. Empty is "None".
    public string Category { get; init; } = "";
    public string Tag { get; init; } = "";
    public string Status { get; init; } = "";
    public string Time { get; init; } = "";
    public string Campus { get; init; } = "";

    // Hidden Type value.
    public string Type { get; init; } = "";
}