using System;
using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

public class DetailsCommunityViewModel
{
    
    public string Creator { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public virtual ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();
}
