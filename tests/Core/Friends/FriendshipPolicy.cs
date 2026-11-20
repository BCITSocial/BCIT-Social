namespace BCIT_Social.Core.Friends;

/// <summary>Status values stored in FriendRelationship.Status (kept as strings: they are persisted).</summary>
public static class FriendshipStatus
{
    public const string PENDING = "Pending";
    public const string ACCEPTED = "Accepted";
    public const string DENIED = "Denied";
    public const string REMOVED = "Removed";
}

public enum FriendRequestOutcome
{
    /// <summary>No row yet: insert a new Pending relationship.</summary>
    CreateNew,
    /// <summary>A Denied/Removed row exists: reuse it, swap requester/recipient, set Pending.</summary>
    Reactivate,
    AlreadyFriends,
    /// <summary>The current user already sent a request that is still pending.</summary>
    PendingSentByYou,
    /// <summary>The other user already sent the current user a request: they should accept it instead.</summary>
    PendingFromThem,
}

public enum FriendDecision
{
    Accept,
    Deny,
}

/// <summary>
/// The friend-request state machine. ProfileController.AddFriend and SendFriendRequest contained two copies of this logic.
/// </summary>
public static class FriendshipPolicy
{
    public static FriendRequestOutcome EvaluateRequest(
        string? existingStatus, string? existingRequesterId, string currentUserId)
    {
        if (existingStatus is null) return FriendRequestOutcome.CreateNew;

        return existingStatus switch
        {
            FriendshipStatus.ACCEPTED => FriendRequestOutcome.AlreadyFriends,
            FriendshipStatus.PENDING => existingRequesterId == currentUserId
                ? FriendRequestOutcome.PendingSentByYou
                : FriendRequestOutcome.PendingFromThem,
            _ => FriendRequestOutcome.Reactivate,
        };
    }

    /// <summary>Only the exact lowercase tokens "accept" and "deny" are valid (they come from a form field).</summary>
    public static bool TryParseDecision(string? raw, out FriendDecision decision)
    {
        switch (raw)
        {
            case "accept": decision = FriendDecision.Accept; return true;
            case "deny": decision = FriendDecision.Deny; return true;
            default: decision = default; return false;
        }
    }

    public static string StatusFor(FriendDecision decision) =>
        decision == FriendDecision.Accept ? FriendshipStatus.ACCEPTED : FriendshipStatus.DENIED;
}
