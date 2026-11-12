// author: Andre Di Lascio
// Names the Home feed filters. To add one, add its constant here, a case in
// HomeController.LoadFeedAsync, and a chip in Views/Home/Index.cshtml.

namespace BCIT_Social.ViewModels;

public static class FeedFilters
{
    public const string ALL = "all";
    public const string CAMPUS = "campus";
    public const string COMMUNITIES = "communities";
    public const string FRIENDS = "friends";

    private static readonly string[] Known = [ALL, CAMPUS, COMMUNITIES, FRIENDS];

    // Returns the filter in lower case, or "all" when it is missing or unknown.
    public static string Normalize(string? filter)
    {
        var value = filter?.Trim().ToLowerInvariant();
        return value is not null && Known.Contains(value) ? value : ALL;
    }
}
