using BCIT_Social.Core.Communities;
using static BCIT_Social.Core.Communities.CommunityRole;

namespace BCIT_Social.Tests.Unit.Domain;

/// <summary>
/// Tests the community role & authorization rules (fixing Defects D1, D2, and D7).
/// </summary>
public class CommunityPermissionsTests
{
    // T13
    [Theory]
    //          user creator isMember isMod role
    [InlineData(null, "c", false, false, None)]        // signed out
    [InlineData("", "c", true, true, None)]
    [InlineData("c", "c", false, false, Owner)]       // creator is owner even without a membership row
    [InlineData("c", "c", true, true, Owner)]       //  a moderator flag never downgrades them
    [InlineData("u", "c", false, true, None)]        // stale moderator flag without membership is ignored
    [InlineData("u", "c", true, false, Member)]
    [InlineData("u", "c", true, true, Moderator)]
    public void RoleOf_CombinesCreatorMembershipAndModeratorFlag(
        string? userId, string creatorId, bool isMember, bool isModerator, CommunityRole expected)
    {
        CommunityPermissions.RoleOf(userId, creatorId, isMember, isModerator).ShouldBe(expected);
    }

    [Theory]
    //          role  edit  delete changeRoles leave
    [InlineData(None, false, false, false, false)]
    [InlineData(Member, false, false, false, true)]
    [InlineData(Moderator, false, false, false, true)]
    [InlineData(Owner, true, true, true, false)]   // owners manage everything but cannot abandon the community
    public void ActionMatrix_OnlyTheOwnerEditsDeletesAndChangesRoles_AndOwnersCannotLeave(
        CommunityRole role, bool edit, bool delete, bool changeRoles, bool leave)
    {
        CommunityPermissions.CanEdit(role).ShouldBe(edit);
        CommunityPermissions.CanDelete(role).ShouldBe(delete);
        CommunityPermissions.CanChangeRoles(role).ShouldBe(changeRoles);
        CommunityPermissions.CanLeave(role).ShouldBe(leave);
    }

    
    [Theory]
    [InlineData(Owner, Member, true)]
    [InlineData(Owner, Moderator, true)]
    [InlineData(Owner, Owner, false)]   //nobody removes the owner, including the owner
    [InlineData(Owner, None, false)]
    [InlineData(Moderator, Member, true)]
    [InlineData(Moderator, Moderator, false)]   // only the owner removes a moderator
    [InlineData(Moderator, Owner, false)]
    [InlineData(Moderator, None, false)]
    [InlineData(Member, Member, false)]   // regular members and outsiders can never kick
    [InlineData(Member, Moderator, false)]
    [InlineData(Member, Owner, false)]
    [InlineData(None, Member, false)]
    [InlineData(None, Moderator, false)]
    [InlineData(None, Owner, false)]
    [InlineData(Member, None, false)]
    [InlineData(None, None, false)]
    public void CanKick_FollowsTheModerationHierarchy(CommunityRole actor, CommunityRole target, bool expected)
    {
        CommunityPermissions.CanKick(actor, target).ShouldBe(expected);
    }
}
