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
    // Newest posts in this community, shown as feed cards under the discussions heading.
    public IReadOnlyList<PostCardViewModel> Posts { get; set; } = [];
}
