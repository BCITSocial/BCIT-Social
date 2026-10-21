using System.Text.Json;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BCIT_Social.Controllers;

// Handles /Messages: the conversation list on the left and the open conversation on the right.
// View only: it shows the page from mock data and has no database, sending, or encryption code.
// TODO(BE): this controller is yours to take over and extend.
public class MessagesController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IWebHostEnvironment env, ILogger<MessagesController> logger)
    {
        _env = env;
        _logger = logger;
    }

    // GET /Messages?c=3&filter=unread&details=true  "c" is the conversation to open; without it the newest one opens.
    // "details" opens the panel about the other person on the right.
    [HttpGet]
    public IActionResult Index(int? c, string? filter, bool details)
    {
        // TODO(BE): replace LoadConversationsFromJson() with the signed-in user's Chatboxes (newest first),
        // with message text decrypted and IsMine/IsRead set for the current user.
        var conversations = LoadConversationsFromJson();
        var isUnread = filter == "unread";
        var listed = isUnread ? conversations.Where(conversation => conversation.IsUnread).ToList() : conversations;
        var chosen = c is null ? null : conversations.FirstOrDefault(conversation => conversation.Id == c);

        var model = new MessagesViewModel
        {
            Conversations = listed,
            Active = chosen ?? listed.FirstOrDefault() ?? conversations.FirstOrDefault(),
            IsActiveChosen = chosen is not null,
            Filter = isUnread ? "unread" : "all",
            ShowDetails = details,
        };
        return View(model);
    }

    // Reads the conversations from wwwroot/content/messages-text.json. No file or no entries means an empty inbox.
    private IReadOnlyList<ConversationViewModel> LoadConversationsFromJson()
    {
        var path = Path.Combine(_env.WebRootPath, "content", "messages-text.json");
        if (!System.IO.File.Exists(path))
        {
            return [];
        }

        try
        {
            var json = System.IO.File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<MessagesContent>(json, JsonOptions)?.Conversations ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read {Path}; showing no conversations.", path);
            return [];
        }
    }
}
