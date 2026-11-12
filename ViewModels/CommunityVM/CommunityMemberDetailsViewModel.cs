using System;

namespace BCIT_Social.ViewModels;

public class CommunityMemberDetailsViewModel
{
    public string Id { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsModerator { get; set; }
    public string CommunityName { get; set; } = null!;
    public int CommunityId { get; set; }
}
