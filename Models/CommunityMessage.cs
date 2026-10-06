using System;
using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class CommunityMessage
{
    public int Id { get; set; }

    public int CommunityId { get; set; }

    public string SenderId { get; set; } = null!;

    public string MessageBody { get; set; } = null!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Community Community { get; set; } = null!;

    public IdentityUser Sender { get; set; } = null!;
}