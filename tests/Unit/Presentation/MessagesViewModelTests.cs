using System.Globalization;
using BCIT_Social.Tests.Support;
using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Presentation;

/// <summary>Link building and time labels of the Messages page (UI logic that lives in C#, not in the Razor view).</summary>
public class MessagesViewModelTests
{
    private static MessagesViewModel Model(bool chosen, string filter, bool showDetails) => new()
    {
        Active = new ConversationViewModel { Id = 7, Name = "Maya L." },
        IsActiveChosen = chosen,
        Filter = filter,
        ShowDetails = showDetails,
    };

    // T07
    [Theory]
    //     chosen filter     details  conv  detailsArg → expected query
    [InlineData(true, "unread", true, null, null, "c=7&filter=unread&details=true")]   // keeps all three
    [InlineData(true, "unread", true, 9, false, "c=9&filter=unread")]                 // switch chat, close details
    [InlineData(true, "all", false, null, true, "c=7&details=true")]                  // "all" is the default, never in the URL
    [InlineData(false, "all", false, null, null, "")]                                  // nothing chosen → clean /Messages
    [InlineData(false, "all", false, 3, null, "c=3")]
    public void RouteValues_PreservesFilterConversationAndDetails_UnlessOverridden(
        bool chosen, string filter, bool showDetails, int? conversation, bool? details, string expected)
    {
        var values = Model(chosen, filter, showDetails).RouteValues(conversation, details);

        TestHelpers.Format(values).ShouldBe(expected);
    }

    // T08
    [Fact]
    public void ListTime_PicksTheFormatByAge()
    {
        using var _ = new CultureScope();
        var today = DateTime.Today;
        var inv = CultureInfo.InvariantCulture.DateTimeFormat;

        MessagesViewModel.ListTime(today.AddHours(10).AddMinutes(24)).ShouldBe("10:24 AM");
        MessagesViewModel.ListTime(today.AddDays(-1).AddHours(23)).ShouldBe("Yesterday");

        var threeDaysAgo = today.AddDays(-3);
        MessagesViewModel.ListTime(threeDaysAgo).ShouldBe(threeDaysAgo.DayOfWeek.ToString()[..3]);

        var weekAgo = today.AddDays(-7);
        MessagesViewModel.ListTime(weekAgo).ShouldBe($"{inv.GetAbbreviatedMonthName(weekAgo.Month)} {weekAgo.Day}");
    }

    // T09
    [Fact]
    public void DayLabel_SaysTodayAndYesterday_ThenWeekdayAndDate()
    {
        using var _ = new CultureScope();
        var today = DateTime.Today;
        var inv = CultureInfo.InvariantCulture.DateTimeFormat;

        MessagesViewModel.DayLabel(today.AddHours(9)).ShouldBe("Today");
        MessagesViewModel.DayLabel(today.AddDays(-1)).ShouldBe("Yesterday");

        var older = today.AddDays(-10);
        MessagesViewModel.DayLabel(older)
            .ShouldBe($"{older.DayOfWeek}, {inv.GetAbbreviatedMonthName(older.Month)} {older.Day}");
    }
}
