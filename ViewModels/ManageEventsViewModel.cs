using BCIT_Social.Models;

namespace BCIT_Social.ViewModels;

public class ManageEventsViewModel
{
    public CommunityEvent Event { get; init; } = null!;
    public IReadOnlyList<Community> OwnedCommunities { get; init; } = [];
    public DateTime MinimumEventDate { get; init; }
}
