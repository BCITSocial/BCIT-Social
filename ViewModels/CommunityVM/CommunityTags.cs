namespace BCIT_Social.ViewModels;

// How a tag badge is colored (topic: blue, statuses: green, times: orange).
public enum CommunityTagKind
{
    Topic,
    Status,
    Time,
}

// One colored tag badge on a card, Details, or Delete page.
public record CommunityTagBadge(string Text, CommunityTagKind Kind)
{
    public string CssClass => Kind switch
    {
        CommunityTagKind.Status => "status-badge",
        CommunityTagKind.Time => "time-badge",
        _ => "",
    };
}

// A community's tags, read from Community.Type.
public class CommunityTags
{
    // Matching option's spelling, or null when community has none.
    public string? Category { get; private init; }
    public string? Tag { get; private init; }
    public string? Status { get; private init; }
    public string? Time { get; private init; }
    public string? Campus { get; private init; }

    // Every tag except the campus, in saved order.
    public IReadOnlyList<CommunityTagBadge> Badges { get; private init; } = [];

    // Blue badge (category, tag).
    public IReadOnlyList<string> Topics => Badges
        .Where(badge => badge.Kind == CommunityTagKind.Topic)
        .Select(badge => badge.Text)
        .ToList();

    public static CommunityTags Parse(string? type, CommunityOptions options)
    {
        string? category = null, tag = null, status = null, time = null, campus = null;
        var badges = new List<CommunityTagBadge>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Default value ("??").
        var parts = (type ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (!seen.Add(part) || options.IsNoValue(part))
            {
                continue;
            }

            if (Find(options.Campuses, part) is { } campusOption)
            {
                campus ??= campusOption;
            }
            else if (Find(options.Statuses, part) is { } statusOption)
            {
                status ??= statusOption;
                badges.Add(new CommunityTagBadge(part, CommunityTagKind.Status));
            }
            else if (Find(options.Times, part) is { } timeOption)
            {
                time ??= timeOption;
                badges.Add(new CommunityTagBadge(part, CommunityTagKind.Time));
            }
            else
            {
                // Some values are both a category and tag. ->
                // The first one saved is the category, next one is tag.
                if (category is null && Find(options.Categories, part) is { } categoryOption)
                {
                    category = categoryOption;
                }
                else if (tag is null && Find(options.Tags, part) is { } tagOption)
                {
                    tag = tagOption;
                }
                badges.Add(new CommunityTagBadge(part, CommunityTagKind.Topic));
            }
        }

        return new CommunityTags
        {
            Category = category,
            Tag = tag,
            Status = status,
            Time = time,
            Campus = campus,
            Badges = badges,
        };
    }

    // Option that matches value.
    private static string? Find(IReadOnlyList<string> options, string value) =>
        options.FirstOrDefault(option => string.Equals(option, value, StringComparison.OrdinalIgnoreCase));
}