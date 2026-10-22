using System;
using BCIT_Social.Models;
using Humanizer;

namespace BCIT_Social.ViewModels;

public class DetailsCommunityViewModel
{

    public int Id { get; set; }
    public string Creator { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public virtual ICollection<CommunityMember> Members { get; set; } = [];
}
