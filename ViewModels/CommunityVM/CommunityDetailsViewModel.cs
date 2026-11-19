using BCIT_Social.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    // The broad category shown on the banner, or null when none.
    public string? Category { get; set; }
    // True for the creator: shows Settings (edit + delete) and hides Leave.
    public bool IsCreator { get; set; }
    // Cover image URL for the banner, or null when none was uploaded.
    public string? BannerUrl { get; set; }
    // Settings pop-up form. Null unless the signed-in user is the creator.
    public CommunityFormViewModel? SettingsForm { get; set; }
    // Posts in this community, newest first.
    public IReadOnlyList<PostCardViewModel> Posts { get; set; } = [];
    // Post type choices for the create post pop-up (members only).
    public IReadOnlyList<SelectListItem> PostTypes { get; set; } = [];
    // ---------------------
}
