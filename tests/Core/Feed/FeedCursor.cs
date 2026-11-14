namespace BCIT_Social.Core.Feed;

/// <summary>
/// Keyset-pagination position of the last card the reader has seen: (CreatedAt, Id).
/// Built from untrusted query-string values; <c>new DateTime(long)</c> throws for out-of-range ticks
/// (HomeController.Feed currently turns that into an HTTP 500).
/// </summary>
public readonly record struct FeedCursor(DateTime CreatedAt, int Id)
{
    public static bool TryCreate(long ticks, int id, out FeedCursor cursor)
    {
        if (ticks <= 0 || ticks > DateTime.MaxValue.Ticks || id <= 0)
        {
            cursor = default;
            return false;
        }

        cursor = new FeedCursor(new DateTime(ticks), id);
        return true;
    }

    /// <summary>True when a post sorts strictly after this cursor in "newest first" order.</summary>
    public bool IsOlder(DateTime createdAt, int id) =>
        createdAt < CreatedAt || (createdAt == CreatedAt && id < Id);
}
