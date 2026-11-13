using BCIT_Social.Controllers;
using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Hubs;

/// <summary>
/// SignalR hub that saves messages from chatbox members and delivers them
/// live to every member of that chatbox, for both direct and group chats.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private const string FALLBACK_NAME = "BCIT Student";

    private readonly ApplicationDbContext _db;

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 12, 2026
    /// Description: Creates the hub with the database context used to check membership and save messages.
    /// </summary>
    /// <param name="db">Database context supplied by dependency injection.</param>
    public ChatHub(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: Nov. 12, 2026
    /// Description: Validates and saves a message from a chatbox member, then sends it
    /// as "ReceiveMessage" to every member of that chatbox, including the sender's other tabs.
    /// Throws a HubException if the user is unknown, not a member, or the text is invalid.
    /// </summary>
    /// <param name="chatboxId">Id of the chatbox the message is sent to.</param>
    /// <param name="text">Message text; trimmed, must be 1 to MESSAGE_MAX_LENGTH characters.</param>
    public async Task SendMessage(int chatboxId, string text)
    {
        string? userId = Context.UserIdentifier;

        if (userId == null)
        {
            throw new HubException("User could not be identified.");
        }

        string trimmedText = (text ?? string.Empty).Trim();

        if (trimmedText.Length == 0 || trimmedText.Length > MessagesController.MESSAGE_MAX_LENGTH)
        {
            throw new HubException($"Messages must contain 1 to {MessagesController.MESSAGE_MAX_LENGTH} characters.");
        }

        List<string> memberIds = await _db.ChatboxUsers
            .Where(member => member.ChatboxId == chatboxId)
            .Select(member => member.UserId)
            .ToListAsync();

        if (!memberIds.Contains(userId))
        {
            throw new HubException("You are not a member of this chat.");
        }

        Message message = new Message
        {
            ChatboxId = chatboxId,
            SenderId = userId,
            Text = trimmedText,
            CreatedAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        string? profileName = await _db.Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.DisplayName)
            .FirstOrDefaultAsync();

        string senderName = string.IsNullOrWhiteSpace(profileName)
            ? Context.User?.Identity?.Name ?? FALLBACK_NAME
            : profileName;

        await Clients.Users(memberIds).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.ChatboxId,
            message.SenderId,
            SenderName = senderName,
            message.Text,
            message.CreatedAt
        });
    }
}