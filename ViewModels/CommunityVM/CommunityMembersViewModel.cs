using System;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

public class CommunityMembersViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string CreatorId { get; set; } = null!;

    // Specifically the current user
    public bool IsMod { get; set; }
    public bool IsCreator { get; set; }
    public virtual ICollection<CommunityMember> Members { get; set; } = [];
    public virtual ICollection<CommunityMember> Moderators { get; set; } = [];
}

