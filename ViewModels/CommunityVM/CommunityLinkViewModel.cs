namespace BCIT_Social.ViewModels;

// A community the signed-in user belongs to, shown in the Home "Your communities" row and the sidebar.
public class CommunityLinkViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    // Community category, for example "Interest" or "Study".
    public string Type { get; init; } = "";
    public int MemberCount { get; init; }

    public string Initials => DisplayFormat.Initials(Name);
}
