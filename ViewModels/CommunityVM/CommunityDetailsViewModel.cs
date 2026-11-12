using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

public class CommunityDetailsViewModel
{

    public int Id { get; set; }
    public string Creator { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsMember { get; set; }
    public bool IsMod { get; set; }
    public virtual ICollection<CommunityMember> Members { get; set; } = [];


    // Author: Jay ---------------------
    // Page text from community-details-text.json.
    public CommunityDetailsText Text { get; set; } = new();
    // The description or default when none.
    public string About { get; set; } = "";
    // Tag badges from Type, campus or default campus.
    public IReadOnlyList<CommunityTagBadge> Badges { get; set; } = [];
    public string Location { get; set; } = "";
    // Create popups form.
    public CommunityFormViewModel CreateForm { get; set; } = new();
    // ---------------------
}
