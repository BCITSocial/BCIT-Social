using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BCIT_Social.Hubs;

[Authorize]
public class ChatHub : Hub
{
}