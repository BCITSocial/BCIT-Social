namespace BCIT_Social.ViewModels;

// The shape of wwwroot/content/messages-text.json. One entry in "conversations" makes one row in the list.
public class MessagesContent
{
    public IReadOnlyList<ConversationViewModel> Conversations { get; init; } = [];
}
