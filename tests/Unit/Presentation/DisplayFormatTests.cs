using BCIT_Social.Tests.Support;
using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Presentation;

/// <summary>Shared by every card, comment, avatar and sidebar item – a bug here is visible on every page.</summary>
public class DisplayFormatTests
{
    private static readonly DateTime Now = new(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc);

    // T01
    [Theory]
    [InlineData("Maya Chen", "MC")]
    [InlineData("maya", "M")]
    [InlineData("Maya Lee Chen", "ML")]                // never more than two letters
    [InlineData("maya.chen@my.bcit.ca", "MC")]         // user name fallback is an e-mail address
    [InlineData("a.b@x", "AB")]
    [InlineData("élan vital", "ÉV")]                   
    [InlineData("   ", "")]
    [InlineData("", "")]
    public void Initials_TakesFirstLettersOfFirstTwoWords(string name, string expected)
    {
        DisplayFormat.Initials(name).ShouldBe(expected);
    }

    // T02
    public static TheoryData<DateTime, string> TimeAgoCases => new()
    {
        { Now.AddSeconds(-30), "just now" },
        { Now.AddSeconds(-59), "just now" },
        { Now.AddSeconds(-60), "1m ago" },
        { Now.AddMinutes(-59).AddSeconds(-59), "59m ago" },
        { Now.AddHours(-1), "1h ago" },
        { Now.AddHours(-23).AddMinutes(-59), "23h ago" },
        { Now.AddHours(-24), "1d ago" },
        { Now.AddDays(-6).AddHours(-23), "6d ago" },
        { Now.AddDays(-7), "Nov 5" },                              // switches to a calendar date at 7 days
        { new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc), "Jan 2" },
        { new DateTime(2025, 12, 31, 8, 0, 0, DateTimeKind.Utc), "Dec 31, 2025" }, // previous year shows the year
        { Now.AddMinutes(5), "just now" },                         // clock skew: a "future" post must not show a negative age
    };

    [Theory]
    [MemberData(nameof(TimeAgoCases))]
    public void TimeAgo_FormatsByAge_WithBoundariesAtEachUnit(DateTime createdAt, string expected)
    {
        using var _ = new CultureScope();

        DisplayFormat.TimeAgo(createdAt, Now).ShouldBe(expected);
    }

    // T03
    [Theory]
    [InlineData(0, "0")]
    [InlineData(856, "856")]
    [InlineData(999, "999")]
    [InlineData(1000, "1k")]
    [InlineData(1200, "1.2k")]
    [InlineData(12_345, "12.3k")]
    public void Count_ShowsCompactThousands(int value, string expected)
    {
        using var _ = new CultureScope();

        DisplayFormat.Count(value).ShouldBe(expected);
    }
}
