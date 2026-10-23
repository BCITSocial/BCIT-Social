using System;
using BCIT_Social.Models;
using Humanizer;

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
}
