using System;
using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class CommunityEvent
{
    public int Id { get; set; }

    public string CreatorId { get; set; } = null!;

    public int? CommunityId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public IdentityUser Creator { get; set; } = null!;

    public Community? Community { get; set; }
}