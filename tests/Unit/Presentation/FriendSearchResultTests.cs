using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Presentation;

public class FriendSearchResultTests
{
    [Theory]
    [InlineData(null, null, "None")]
    [InlineData("Removed", "viewer", "None")]
    [InlineData("Denied", "other", "None")]
    [InlineData("Accepted", "other", "Friends")]
    [InlineData("Pending", "viewer", "Sent")]
    [InlineData("Pending", "other", "Received")]
    public void RelationshipStatus_ReflectsTheViewersAction(string? status, string? requesterId, string expected)
    {
        FriendSearchResultViewModel.GetRelationshipStatus(status, requesterId, "viewer").ShouldBe(expected);
    }
}
