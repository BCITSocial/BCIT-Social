using System.Security.Claims;
using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
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
    private const int MESSAGE_MAX_LENGTH = 5000;
    private readonly ApplicationDbContext _db;

    public MessagesController(ApplicationDbContext db) => _db = db;

    // GET /Messages?c=3&filter=unread&details=true
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
            var other = chat.ChatboxUsers.FirstOrDefault(member => member.UserId != userId)?.User;
            var otherId = other?.Id ?? "";
            profiles.TryGetValue(otherId, out var profile);
            var displayName = profile?.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName)) displayName = other?.Email ?? "Alumni member";

            var messages = chat.Messages
                .OrderBy(message => message.CreatedAt)
                .Select(message => new ChatMessageViewModel
                {
                    Id = message.Id,
                    Text = message.Text,
                    SentAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc).ToLocalTime(),
                    IsMine = message.SenderId == userId,
                    IsRead = false,
                })
                .ToArray();

            return new ConversationViewModel
            {
                Id = chat.Id,
                UserId = otherId,
                Name = displayName,
                Program = profile?.Program ?? "",
                PhotoUrl = null,
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

        var isMember = await _db.ChatboxUsers.AnyAsync(
            member => member.ChatboxId == conversationId && member.UserId == userId,
            cancellationToken);
        if (!isMember) return NotFound();

        // Take the sender identity from the authenticated session, never from submitted form data.
        _db.Messages.Add(new Message
        {
            ChatboxId = conversationId,
            SenderId = userId,
            Text = body,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index), new { c = conversationId });
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartConversation(string? recipientId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        if (string.IsNullOrWhiteSpace(recipientId) || recipientId == userId) return BadRequest();

        var recipientExists = await _db.Users.AnyAsync(user => user.Id == recipientId, cancellationToken);
        if (!recipientExists) return NotFound();

        // Reuse an existing two-person conversation to avoid duplicate inbox entries.
        var existingId = await _db.Chatboxes
            .Where(chat => chat.ChatboxUsers.Any(member => member.UserId == userId)
                && chat.ChatboxUsers.Any(member => member.UserId == recipientId)
                && chat.ChatboxUsers.Count == 2)
            .Select(chat => (int?)chat.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingId is not null)
            return RedirectToAction(nameof(Index), new { c = existingId.Value });

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

        return RedirectToAction(nameof(Index), new { c = chatbox.Id });
    }
}
