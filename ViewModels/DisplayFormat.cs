using System.Globalization;

namespace BCIT_Social.ViewModels;

// Shared text formatting for names and times shown on cards, comments, and the sidebar.
public static class DisplayFormat
{
    // Up to two letters for an avatar, for example "Maya Chen" -> "MC".
    public static string Initials(string name)
    {
        var parts = name.Split([' ', '.', '@'], StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
    }

    // Short relative time from a UTC date, for example "5m ago", "2h ago", "3d ago", or "Oct 2" for older dates.
    public static string TimeAgo(DateTime createdAt, DateTime utcNow)
    {
        var age = utcNow - createdAt;
        if (age.TotalMinutes < 1) return "just now";
        if (age.TotalHours < 1) return $"{(int)age.TotalMinutes}m ago";
        if (age.TotalDays < 1) return $"{(int)age.TotalHours}h ago";
        if (age.TotalDays < 7) return $"{(int)age.TotalDays}d ago";
        // Older posts show the date and the hour, in campus (Vancouver) time, for example "Oct 22, 3:45 PM".
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc), CampusTimeZone);
        return local.Year == utcNow.Year
            ? local.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture)
            : local.ToString("MMM d, yyyy, h:mm tt", CultureInfo.InvariantCulture);
    }

    // BCIT's time zone. "America/Vancouver" works on Windows (.NET 6+) and Linux.
    private static readonly TimeZoneInfo CampusTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");

    // Compact member count, for example 856 -> "856", 1200 -> "1.2k".
    public static string Count(int value) =>
        value >= 1000 ? $"{value / 1000.0:0.#}k" : value.ToString();
}
