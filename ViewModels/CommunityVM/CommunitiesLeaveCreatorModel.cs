using System;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

public class CommunitiesLeaveCreatorModel
{
    public int Id { get; set; }
    public string CommunityName { get; set; } = null!;
    public string CreatorId { get; set; } = "";
    public int MemberCount { get; set; }
    public virtual ICollection<CommunityMember> Members { get; set; } = [];
}
