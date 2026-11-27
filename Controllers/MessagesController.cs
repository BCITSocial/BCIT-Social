using System.Security.Claims;
using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using BCIT_Social.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Controllers;

// @author Asher
// Provides the signed-in user's direct-message inbox, recipient lookup, and send actions.
[Authorize]
public class MessagesController : Controller
{
    private const int MESSAGE_PAGE_SIZE = 100;
    public const int MESSAGE_MAX_LENGTH = 5000;
    // A group needs the creator plus at least two others; the cap keeps the member list readable.
    private const int GROUP_MIN_OTHERS = 2;
    private const int GROUP_MAX_OTHERS = 24;
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<ChatHub> _hub;

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 26, 2026
    /// Description: Creates the controller with the database context and the hub context used to push live messages.
    /// </summary>
    /// <param name="db" type="ApplicationDbContext">Database context supplied by dependency injection.</param>
    /// <param name="hub" type="IHubContext&lt;ChatHub&gt;">Chat hub context used to send saved messages to chatbox members.</param>
    public MessagesController(ApplicationDbContext db, IHubContext<ChatHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 26, 2026
    /// Description: Shows the signed-in user's inbox: every chatbox they belong to with its latest messages, with one conversation open and the details panel optionally shown. Route: GET /Messages?c=3.
    /// </summary>
    /// <param name="c" type="int?">Id of the chatbox to open; the first listed conversation opens when null or not found.</param>
    /// <param name="filter" type="string?">"unread" to list only unread conversations; any other value lists all of them.</param>
    /// <param name="details" type="bool">True to show the details panel for the open conversation.</param>
    /// <param name="cancellationToken" type="CancellationToken">Token that cancels the database work if the request is aborted.</param>
    /// <return type="Task&lt;IActionResult&gt;">The inbox view, or a sign-in challenge when the user cannot be identified.</return>
    [HttpGet]
    public async Task<IActionResult> Index(int? c, string? filter, bool details, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();

        // Load only conversations the current user belongs to, with a bounded recent message history.
        var chatboxes = await _db.Chatboxes
            .AsNoTracking()
            .Where(chat => chat.ChatboxUsers.Any(member => member.UserId == userId))
            .Include(chat => chat.ChatboxUsers)
                .ThenInclude(member => member.User)
            .Include(chat => chat.Messages.OrderByDescending(message => message.CreatedAt).Take(MESSAGE_PAGE_SIZE))
                .ThenInclude(message => message.Sender)
            .ToListAsync(cancellationToken);

        var otherUserIds = chatboxes
            .SelectMany(chat => chat.ChatboxUsers)
            .Select(member => member.UserId)
            .Where(id => id != userId)
            .Distinct()
            .ToArray();

        var profiles = await _db.Profiles.AsNoTracking()
            .Where(profile => otherUserIds.Contains(profile.UserId))
            .ToDictionaryAsync(profile => profile.UserId, cancellationToken);

        var conversations = chatboxes.Select(chat =>
        {
            // A chatbox with more than two members is a group chat (the current schema has no type column yet).
            var others = chat.ChatboxUsers
                .Where(member => member.UserId != userId)
                .Select(member => new ChatMemberViewModel
                {
                    UserId = member.UserId,
                    Name = NameFor(member.UserId, member.User, profiles),
                    Program = profiles.TryGetValue(member.UserId, out var memberProfile) ? memberProfile.Program ?? "" : "",
                })
                .OrderBy(member => member.Name)
                .ToList();
            var isGroup = chat.ChatboxUsers.Count > 2;
            var other = others.FirstOrDefault();

            var messages = chat.Messages
                .OrderBy(message => message.CreatedAt)
                .Select(message => new ChatMessageViewModel
                {
                    Id = message.Id,
                    Text = message.Text,
                    SentAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc).ToLocalTime(),
                    IsMine = message.SenderId == userId,
                    IsRead = false,
                    SenderName = message.SenderId == userId ? "You" : NameFor(message.SenderId, message.Sender, profiles),
                })
                .ToArray();

            return new ConversationViewModel
            {
                Id = chat.Id,
                UserId = isGroup ? "" : other?.UserId ?? "",
                Name = isGroup ? GroupName(others) : other?.Name ?? "Alumni member",
                Program = isGroup ? $"{others.Count + 1} members" : other?.Program ?? "",
                PhotoUrl = isGroup || other is null
                    || !profiles.TryGetValue(other.UserId, out var otherProfile)
                    || string.IsNullOrWhiteSpace(otherProfile.ProfilePhotoStorageKey)
                    ? null
                    : Url.Action("Photo", "Profile", new { id = other.UserId }),
                IsGroup = isGroup,
                Members = others,
                Messages = messages,
            };
        }).OrderByDescending(conversation => conversation.LastMessage?.SentAt ?? DateTime.MinValue).ToList();

        var isUnread = string.Equals(filter, "unread", StringComparison.OrdinalIgnoreCase);
        // Read receipts are not represented in the current schema yet, so the unread filter remains empty.
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

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 26, 2026
    /// Description: Saves a message from a chatbox member and sends it live to every member through the chat hub. Used as the fallback when the page cannot send through SignalR.
    /// </summary>
    /// <param name="conversationId" type="int">Id of the chatbox the message is sent to.</param>
    /// <param name="text" type="string?">Message text; trimmed, must be 1 to MESSAGE_MAX_LENGTH characters.</param>
    /// <param name="cancellationToken" type="CancellationToken">Token that cancels the database work if the request is aborted.</param>
    /// <return type="Task&lt;IActionResult&gt;">A redirect back to the conversation, NotFound if the user is not a member, or a sign-in challenge.</return>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int conversationId, string? text, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();

        var body = text?.Trim();
        if (string.IsNullOrWhiteSpace(body) || body.Length > MESSAGE_MAX_LENGTH)
        {
            TempData["MessageError"] = $"Messages must contain 1 to {MESSAGE_MAX_LENGTH} characters.";
            return RedirectToAction(nameof(Index), new { c = conversationId });
        }

        // Load every member once: this checks membership and tells us who to notify.
        var memberIds = await _db.ChatboxUsers
            .Where(member => member.ChatboxId == conversationId)
            .Select(member => member.UserId)
            .ToListAsync(cancellationToken);
        if (!memberIds.Contains(userId)) return NotFound();

        // Take the sender identity from the authenticated session, never from submitted form data.
        var message = new Message
        {
            ChatboxId = conversationId,
            SenderId = userId,
            Text = body,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Messages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var profileName = await _db.Profiles.AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.DisplayName)
            .FirstOrDefaultAsync(cancellationToken);

        // Same payload as ChatHub.SendMessage, so the page handles both the same way.
        await _hub.Clients.Users(memberIds).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.ChatboxId,
            message.SenderId,
            SenderName = string.IsNullOrWhiteSpace(profileName) ? User.Identity?.Name ?? "Alumni member" : profileName,
            message.Text,
            message.CreatedAt,
        }, cancellationToken);

        return RedirectToAction(nameof(Index), new { c = conversationId });
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 26, 2026
    /// Description: Searches other users' profiles by display name for the new-message recipient picker.
    /// </summary>
    /// <param name="q" type="string?">Search text; at least 2 characters after trimming, otherwise no results are returned.</param>
    /// <param name="cancellationToken" type="CancellationToken">Token that cancels the database work if the request is aborted.</param>
    /// <return type="Task&lt;IActionResult&gt;">JSON list of up to 20 matches with id, name, and program, or a sign-in challenge.</return>
    [HttpGet]
    public async Task<IActionResult> FindUsers(string? q, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null) return Challenge();
        var query = q?.Trim();
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Json(Array.Empty<object>());

        // Only expose other users with profiles, and keep the recipient picker results bounded.
        var matches = await _db.Profiles.AsNoTracking()
            .Where(profile => profile.UserId != currentUserId && profile.DisplayName.Contains(query))
            .OrderBy(profile => profile.DisplayName)
            .Select(profile => new { id = profile.UserId, name = profile.DisplayName, program = profile.Program })
            .Take(20)
            .ToListAsync(cancellationToken);

        return Json(matches);
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 26, 2026
    /// Description: Opens the direct chat between the signed-in user and the chosen recipient, creating it when none exists.
    /// </summary>
    /// <param name="recipientId" type="string?">User id of the person to message; must be another existing user.</param>
    /// <param name="cancellationToken" type="CancellationToken">Token that cancels the database work if the request is aborted.</param>
    /// <return type="Task&lt;IActionResult&gt;">A redirect to the conversation, BadRequest for a missing or own id, NotFound for an unknown user, or a sign-in challenge.</return>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartConversation(string? recipientId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        if (string.IsNullOrWhiteSpace(recipientId) || recipientId == userId) return BadRequest();

        var recipientExists = await _db.Users.AnyAsync(user => user.Id == recipientId, cancellationToken);
        if (!recipientExists) return NotFound();

        var chatboxId = await FindOrCreateDirectChatAsync(userId, recipientId, cancellationToken);
        return RedirectToAction(nameof(Index), new { c = chatboxId });
    }

    // POST /Messages/MessageUser
    // The quick "Message @name" box on a user's avatar popout (Discord style): opens the direct chat with that
    // person, creating it if needed, and sends the typed message straight away when there is one.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MessageUser(string? recipientId, string? text, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        if (string.IsNullOrWhiteSpace(recipientId) || recipientId == userId) return BadRequest();

        var recipientExists = await _db.Users.AnyAsync(user => user.Id == recipientId, cancellationToken);
        if (!recipientExists) return NotFound();

        var body = text?.Trim();
        if (body is { Length: > MESSAGE_MAX_LENGTH }) return BadRequest();

        var chatboxId = await FindOrCreateDirectChatAsync(userId, recipientId, cancellationToken);
        if (!string.IsNullOrEmpty(body))
        {
            _db.Messages.Add(new Message
            {
                ChatboxId = chatboxId,
                SenderId = userId,
                Text = body,
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        return RedirectToAction(nameof(Index), new { c = chatboxId });
    }

    // POST /Messages/CreateGroup
    // Creates a group chat with the signed-in user and the chosen people.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup(List<string>? memberIds, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();

        var others = (memberIds ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id) && id != userId)
            .Distinct()
            .ToList();
        if (others.Count < GROUP_MIN_OTHERS || others.Count > GROUP_MAX_OTHERS)
        {
            TempData["MessageError"] = $"A group chat needs {GROUP_MIN_OTHERS} to {GROUP_MAX_OTHERS} other people.";
            return RedirectToAction(nameof(Index));
        }

        var existingCount = await _db.Users.CountAsync(user => others.Contains(user.Id), cancellationToken);
        if (existingCount != others.Count) return NotFound();

        var chatbox = new Chatbox
        {
            CreatorId = userId,
            CreatedAt = DateTime.UtcNow,
            ChatboxUsers = others.Prepend(userId).Select(id => new ChatboxUser { UserId = id }).ToList(),
        };
        _db.Chatboxes.Add(chatbox);
        await _db.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index), new { c = chatbox.Id });
    }

    // Returns the two-person chat between the users, creating it when there is none, so the inbox has no duplicates.
    private async Task<int> FindOrCreateDirectChatAsync(string userId, string recipientId, CancellationToken cancellationToken)
    {
        var existingId = await _db.Chatboxes
            .Where(chat => chat.ChatboxUsers.Any(member => member.UserId == userId)
                && chat.ChatboxUsers.Any(member => member.UserId == recipientId)
                && chat.ChatboxUsers.Count == 2)
            .Select(chat => (int?)chat.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingId is not null) return existingId.Value;

        var chatbox = new Chatbox
        {
            CreatorId = userId,
            CreatedAt = DateTime.UtcNow,
            ChatboxUsers =
            [
                new ChatboxUser { UserId = userId },
                new ChatboxUser { UserId = recipientId },
            ],
        };
        _db.Chatboxes.Add(chatbox);
        await _db.SaveChangesAsync(cancellationToken);
        return chatbox.Id;
    }

    // Profile display name, then the account email, then a placeholder.
    private static string NameFor(string? id, ApplicationUser? user, IReadOnlyDictionary<string, Profile> profiles)
    {
        if (id is not null && profiles.TryGetValue(id, out var profile) && !string.IsNullOrWhiteSpace(profile.DisplayName))
            return profile.DisplayName;
        return user?.Email ?? "Alumni member";
    }

    // "Maya L., Sam K. and Jo P." for up to three people, otherwise "Maya L., Sam K. and 3 others".
    private static string GroupName(IReadOnlyList<ChatMemberViewModel> others)
    {
        var first = others.Select(member => member.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? member.Name).ToList();
        return first.Count switch
        {
            0 => "Group chat",
            1 => first[0],
            2 or 3 => string.Join(", ", first.Take(first.Count - 1)) + " and " + first[^1],
            _ => $"{first[0]}, {first[1]} and {first.Count - 2} others",
        };
    }
}
