namespace BCIT_Social.Core.Friends;

/// <summary>
/// Order-independent key for a pair of users. The FriendRelationships table has a UNIQUE index on this value,
/// so A→B and B→A must always map to the same string.
/// (Extracted from ProfileController.GetRelationshipKey.)
/// </summary>
public static class FriendshipKey
{
    public static string For(string firstUserId, string secondUserId)
    {
        ArgumentException.ThrowIfNullOrEmpty(firstUserId);
        ArgumentException.ThrowIfNullOrEmpty(secondUserId);

        return string.CompareOrdinal(firstUserId, secondUserId) < 0
            ? $"{firstUserId}:{secondUserId}"
            : $"{secondUserId}:{firstUserId}";
    }
}
