namespace BCIT_Social.Core.Communities;

/// <summary>The relationship between one user and one community.</summary>
public enum CommunityRole
{
    /// <summary>Signed out, or signed in but not a member.</summary>
    None,
    Member,
    Moderator,
    /// <summary>The community creator. Always wins over membership flags.</summary>
    Owner,
}

/// <summary>
/// Single source of truth for "who may do what" in a community.
/// Replaces the scattered checks in CommunityController (CommunityAccessChecker, inline IsModerator checks),
/// and makes the POST actions Kick / EditStatus / Delete impossible to forget to protect: they call the same rule as the GET.
/// </summary>
public static class CommunityPermissions
{
    public static CommunityRole RoleOf(string? userId, string creatorId, bool isMember, bool isModerator)
    {
        if (string.IsNullOrEmpty(userId)) return CommunityRole.None;
        if (userId == creatorId) return CommunityRole.Owner;
        if (!isMember) return CommunityRole.None;
        return isModerator ? CommunityRole.Moderator : CommunityRole.Member;
    }

    public static bool CanEdit(CommunityRole actor) => actor == CommunityRole.Owner;

    public static bool CanDelete(CommunityRole actor) => actor == CommunityRole.Owner;

    public static bool CanChangeRoles(CommunityRole actor) => actor == CommunityRole.Owner;

    /// <summary>Owners cannot leave (the community would have no owner). Non-members have nothing to leave.</summary>
    public static bool CanLeave(CommunityRole actor) => actor is CommunityRole.Member or CommunityRole.Moderator;

    /// <summary>
    /// Owners can remove members and moderators; moderators can remove regular members only;
    /// nobody can remove the owner (including the owner themself).
    /// </summary>
    public static bool CanKick(CommunityRole actor, CommunityRole target) => actor switch
    {
        CommunityRole.Owner => target is CommunityRole.Member or CommunityRole.Moderator,
        CommunityRole.Moderator => target == CommunityRole.Member,
        _ => false,
    };
}
