namespace BCIT_Social.ViewModels;

// One conversation (a Chatbox) as seen by the signed-in user: the other person and the messages.
public class ConversationViewModel
{
    // The Chatbox id. Used in the URL (?c=...) to open the conversation.
    public int Id { get; init; }
    // The other person's user id. Used for the "View profile" link.
    public string UserId { get; init; } = "";
    // The other person's display name, for example "Maya L.".
    public string Name { get; init; } = "";
    public string? PhotoUrl { get; init; }
    // Program or role shown under the name in the chat header, for example "Digital Design".
    public string Program { get; init; } = "";
    public bool IsOnline { get; init; }
    public bool IsTyping { get; init; }
    // True when there are messages the signed-in user has not read yet.
    public bool IsUnread { get; init; }
    // Oldest first.
    public IReadOnlyList<ChatMessageViewModel> Messages { get; init; } = [];
    // Details panel: communities both people belong to, and files sent in this conversation (newest first).
    public IReadOnlyList<CommunityLinkViewModel> SharedCommunities { get; init; } = [];
    public IReadOnlyList<SharedMediaViewModel> SharedMedia { get; init; } = [];
    public bool IsMuted { get; init; }

    public ChatMessageViewModel? LastMessage => Messages.Count > 0 ? Messages[^1] : null;

    public string Initials => DisplayFormat.Initials(Name);
}
