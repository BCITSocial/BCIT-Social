using System;
using System.Collections.Generic;

namespace project.Models.NW;

public partial class CommunityMessage
{
    public int Id { get; set; }

    public int CommunityId { get; set; }

    public int SenderId { get; set; }

    public string MessageBody { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
