using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Hubs;

/// <summary>
/// SignalR hub that lets signed-in members of a chatbox join it,
/// send messages to it, and receive messages sent by other members.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private const int MAX_MESSAGE_LENGTH = 2000;

    private readonly ApplicationDbContext _db;

    /// <summary>
    /// Author: Asher 
    /// Last Revised: 2026-11-12
    /// Description: Creates the hub with the database context it needs to check membership and save messages.
    /// </summary>
    /// <param name="db" type="ApplicationDbContext">Database context supplied by dependency injection.</param>
    public ChatHub(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Author: Asher 
    /// Last Revised: 2026-11-12
    /// Description: Builds the SignalR group name that all connections in one chatbox share.
    /// </summary>
    /// <param name="chatboxId" type="int">Id of the chatbox.</param>
    /// <return type="string">The group name for that chatbox.</return>
    private static string GetGroupName(int chatboxId)
    {
        return $"chatbox-{chatboxId}";
    }

    /// <summary>
    /// Author: Asher 
    /// Last Revised: 2026-11-12
    /// Description: Confirms the connected user belongs to the chatbox and returns their user id.
    /// Throws a HubException if the user is unknown or not a member.
    /// </summary>
    /// <param name="chatboxId" type="int">Id of the chatbox to check membership for.</param>
    /// <return type="Task&lt;string&gt;">The id of the verified member.</return>
    private async Task<string> EnsureMemberAsync(int chatboxId)
    {
        string? userId = Context.UserIdentifier;

        if (userId == null)
        {
            throw new HubException("User could not be identified.");
        }

        bool isMember = await _db.ChatboxUsers
            .AnyAsync(cu => cu.UserId == userId && cu.ChatboxId == chatboxId);

        if (!isMember)
        {
            throw new HubException("You are not a member of this chat.");
        }

        return userId;
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: 2026-11-12
    /// Description: Adds the current connection to the chatbox group so it receives that chatbox's messages.
    /// Only members of the chatbox may join.
    /// </summary>
    /// <param name="chatboxId" type="int">Id of the chatbox to join.</param>
    public async Task JoinChatbox(int chatboxId)
    {
        await EnsureMemberAsync(chatboxId);

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(chatboxId));
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: 2026-11-12
    /// Description: Removes the current connection from the chatbox group so it stops receiving messages.
    /// </summary>
    /// <param name="chatboxId" type="int">Id of the chatbox to leave.</param>
    public async Task LeaveChatbox(int chatboxId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(chatboxId));
    }

    /// <summary>
    /// Author: Asher
    /// Last Revised: 2026-11-12
    /// Description: Validates and saves a message from a chatbox member, then broadcasts it
    /// to every connection in that chatbox's group as "ReceiveMessage".
    /// </summary>
    /// <param name="chatboxId" type="int">Id of the chatbox the message is sent to.</param>
    /// <param name="text" type="string">Message text; trimmed, must be 1 to MAX_MESSAGE_LENGTH characters.</param>
    public async Task SendMessage(int chatboxId, string text)
    {
        string userId = await EnsureMemberAsync(chatboxId);

        string trimmedText = (text ?? string.Empty).Trim();

        if (trimmedText.Length == 0 || trimmedText.Length > MAX_MESSAGE_LENGTH)
        {
            throw new HubException($"Message must be 1-{MAX_MESSAGE_LENGTH} characters.");
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

        await Clients.Group(GetGroupName(chatboxId)).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.ChatboxId,
            message.SenderId,
            SenderName = Context.User?.Identity?.Name,
            message.Text,
            message.CreatedAt
        });
    }
}