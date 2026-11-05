using System;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

public class CommunityMembersViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public CommunityMember Creator { get; set; } = null!;
    public bool IsMod { get; set; }
    public virtual ICollection<CommunityMember> Members { get; set; } = [];
    public virtual ICollection<CommunityMember> Moderators { get; set; } = [];
}

