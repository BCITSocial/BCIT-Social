using System;
using System.Collections.Generic;

namespace project.Models.NW;

public partial class CommunityMember
{
    public int UserId { get; set; }

    public int CommunityId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
