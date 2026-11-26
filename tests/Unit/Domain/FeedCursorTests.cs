using BCIT_Social.Core.Feed;

namespace BCIT_Social.Tests.Unit.Domain;

public class FeedCursorTests
{
    private static readonly long SomeTicks = new DateTime(2026, 11, 12, 12, 0, 0).Ticks;

    // T21  (D5: HomeController.Feed throws ArgumentOutOfRangeException → HTTP 500 for these inputs)
    // TEST FULLY GENERATED WITH AI, THIS IS MAGIIIIIC, 
    public static TheoryData<long, int, bool> Cases => new()
    {
        { SomeTicks, 42, true },
        { 1, 1, true },
        { DateTime.MaxValue.Ticks, 1, true },     // upper boundary
        { DateTime.MaxValue.Ticks + 1, 1, false },
        { long.MaxValue, 1, false },
        { 0, 1, false },                          // "no cursor supplied" must not look like a real position
        { -1, 1, false },
        { long.MinValue, 1, false },
        { SomeTicks, 0, false },
        { SomeTicks, -7, false },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void TryCreate_RejectsOutOfRangeValues_InsteadOfThrowing(long ticks, int id, bool expected)
    {
        FeedCursor.TryCreate(ticks, id, out var cursor).ShouldBe(expected);

        if (expected)
        {
            cursor.CreatedAt.Ticks.ShouldBe(ticks);
            cursor.Id.ShouldBe(id);
        }
    }

    // T22
    [Fact]
    public void IsOlder_IsStrict_AndBreaksTiesById()
    {
        var at = new DateTime(2026, 11, 12, 12, 0, 0);
        FeedCursor.TryCreate(at.Ticks, 50, out var cursor).ShouldBeTrue();

        cursor.IsOlder(at.AddSeconds(-1), 999).ShouldBeTrue();   // older time wins regardless of id
        cursor.IsOlder(at.AddSeconds(1), 1).ShouldBeFalse();     // newer time never qualifies
        cursor.IsOlder(at, 49).ShouldBeTrue();                   // same instant: lower id comes later in the feed
        cursor.IsOlder(at, 50).ShouldBeFalse();                  // the cursor card itself must not repeat
        cursor.IsOlder(at, 51).ShouldBeFalse();
    }
}
