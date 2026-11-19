using System.Text;

namespace BCIT_Social.Tests.Support;

/// <summary>
/// Minimal HTML fixtures that mirror the exact DOM contracts (data-* hooks, classes, IDs, ARIA attributes)
/// of Views/Messages/Index.cshtml, Views/Profile/Index.cshtml, and Views/Home/Index.cshtml.
/// If a Razor view renames a selector expected by wwwroot/js/*.js, these Playwright UI tests catch the mismatch.
/// </summary>
public static class UiDomFixtures
{
    private static string ConversationRow(int id, string name, string preview) =>
        $"""
        <li><a class="msg-conversation" data-conversation-id="{id}" href="/Messages?c={id}">
            <span class="msg-conversation-text">
                <span class="msg-conversation-top"><strong>{name}</strong></span>
                <span class="msg-conversation-bottom"><span class="msg-preview">{preview}</span></span>
            </span></a></li>
        """;

    private static string BubbleRow(string sender, string text) =>
        $"""
        <li class="msg-row"><div class="msg-bubble-group">
            <p class="msg-bubble"><span class="visually-hidden">{sender}: </span>{text}</p></div></li>
        """;

    public static string MessagesPage(params (string Sender, string Text)[] bubbles)
    {
        var bubbleMarkup = string.Concat(bubbles.Select(b => BubbleRow(b.Sender, b.Text)));
        return $"""
            <div class="messages-page is-chat-open" data-messages>
                <dialog data-new-message-dialog>
                    <input type="search" data-recipient-search />
                    <p data-recipient-status>Enter at least 2 characters.</p>
                    <ul data-recipient-results></ul>
                    <form data-start-conversation>
                        <input type="hidden" name="recipientId" />
                        <button type="submit" disabled data-start-conversation-button>Open conversation</button>
                    </form>
                </dialog>

                <button type="button" data-new-message data-find-users-url="/Messages/FindUsers">New message</button>
                <input type="search" data-message-search />
                <p data-message-search-status></p>
                <p hidden data-message-search-empty>No conversations match your search.</p>
                <ul class="msg-conversations">
                    {ConversationRow(1, "Maya L.", "See you at the hackathon")}
                    {ConversationRow(2, "Jordan K.", "Notes attached")}
                    {ConversationRow(3, "Priya S.", "Lunch at noon?")}
                </ul>

                <section class="msg-chat" data-conversation-id="1">
                    <header class="msg-chat-head">
                        <button type="button" aria-expanded="false" data-conversation-search>Search</button>
                        <a data-details-open href="/Messages?c=1&details=true">View details</a>
                    </header>
                    <div class="msg-chat-search" hidden data-chat-search>
                        <input type="search" data-chat-search-input />
                        <span data-chat-search-count></span>
                        <button type="button" data-chat-search-step="-1">Previous</button>
                        <button type="button" data-chat-search-step="1">Next</button>
                        <button type="button" data-chat-search-close>Close</button>
                    </div>
                    <div class="msg-scroll"><ol class="msg-log">
                        {bubbleMarkup}
                        <li class="msg-row msg-typing" hidden data-typing></li>
                    </ol></div>
                    <form data-message-form>
                        <textarea name="text"></textarea>
                        <button class="msg-send" type="submit">Send</button>
                    </form>
                </section>

                <aside id="msg-details" hidden tabindex="-1"><a data-details-close href="/Messages?c=1">Close</a></aside>
            </div>
            """;
    }

    public static string ProfileTabs() =>
        """
        <div role="tablist">
            <button data-profile-tab="posts" aria-selected="true" tabindex="0">Posts</button>
            <button data-profile-tab="bookmarks" aria-selected="false" tabindex="-1">Bookmarks</button>
            <button data-profile-tab="friends" aria-selected="false" tabindex="-1">Friends</button>
        </div>
        <section data-profile-panel="posts">Posts panel</section>
        <section data-profile-panel="bookmarks" hidden>Bookmarks panel</section>
        <section data-profile-panel="friends" hidden>Friends panel</section>
        """;

    public static string PostCard(int id, long ticks) =>
        $"""<article class="post-card" data-post-id="{id}" data-created-ticks="{ticks}"></article>""";

    public static string FeedPage(int cardCount)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < cardCount; i++)
        {
            sb.AppendLine(PostCard(100 - i, 639000000000000000L - i));
        }

        return $"""
            <div id="feed-list" data-has-more="true" data-feed-url="/Home/Feed">
                {sb}
            </div>
            <p id="feed-status"></p>
            <button id="feed-retry" hidden>Try again</button>
            """;
    }
}
