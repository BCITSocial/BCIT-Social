namespace BCIT_Social.ViewModels;

// One message bubble in the open conversation.
public class ChatMessageViewModel
{
    public int Id { get; init; }
    // Plain (already decrypted) message text.
    public string Text { get; init; } = "";
    public DateTime SentAt { get; init; }
    // True when the signed-in user sent it. Mine are shown on the right in blue.
    public bool IsMine { get; init; }
    // Only used on my messages: shows the double check when the other person has read it.
    public bool IsRead { get; init; }
    // Display name of the sender ("You" for mine). Shown above their bubbles in group chats.
    public string SenderName { get; init; } = "";

    public string SenderInitials => DisplayFormat.Initials(SenderName);
}
