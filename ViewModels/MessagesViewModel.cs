namespace BCIT_Social.ViewModels;

// Everything the Messages page shows: the conversation list and the open conversation.
public class MessagesViewModel
{
    // Newest conversation first.
    public IReadOnlyList<ConversationViewModel> Conversations { get; init; } = [];
    // Null when there are no conversations.
    public ConversationViewModel? Active { get; init; }
    // True when the URL picked the conversation (?c=...). Phones then show the chat instead of the list.
    public bool IsActiveChosen { get; init; }
    // "all" or "unread".
    public string Filter { get; init; } = "all";
    // True when the details panel is open (?details=true).
    public bool ShowDetails { get; init; }
    // Route values for a link that keeps the current filter, conversation, and details state unless overridden.
    public Dictionary<string, string> RouteValues(int? conversation = null, bool? details = null)
    {
        var values = new Dictionary<string, string>();
        var id = conversation ?? (IsActiveChosen ? Active?.Id : null);
        if (id is not null) values["c"] = id.Value.ToString();
        if (Filter != "all") values["filter"] = Filter;
        if (details ?? ShowDetails) values["details"] = "true";
        return values;
    }

    public static readonly IReadOnlyList<(string Value, string Label)> Filters = [("all", "All"), ("unread", "Unread")];

    // List row time: "10:24 AM" today, "Yesterday", the weekday within a week, otherwise "Oct 3".
    public static string ListTime(DateTime sentAt)
    {
        var days = (DateTime.Today - sentAt.Date).Days;
        return days switch
        {
            0 => sentAt.ToString("h:mm tt"),
            1 => "Yesterday",
            < 7 => sentAt.ToString("ddd"),
            _ => sentAt.ToString("MMM d"),
        };
    }

    // Divider between days in the chat: "Today", "Yesterday", otherwise "Monday, Oct 19".
    public static string DayLabel(DateTime day)
    {
        var days = (DateTime.Today - day.Date).Days;
        return days switch
        {
            0 => "Today",
            1 => "Yesterday",
            _ => day.ToString("dddd, MMM d"),
        };
    }
}
