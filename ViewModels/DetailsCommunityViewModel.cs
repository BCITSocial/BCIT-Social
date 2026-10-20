using System;
using BCIT_Social.Models;
using Humanizer;

namespace BCIT_Social.ViewModels;

public class DetailsCommunityViewModel
{

    public string Creator { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<CommunityMember> Members { get; set; } = [];
}
