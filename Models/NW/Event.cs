using System;
using System.Collections.Generic;

namespace project.Models.NW;

public partial class Event
{
    public int Id { get; set; }

    public int CreatorId { get; set; }

    public int? CommunityId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime? EventDate { get; set; }

    public virtual Community? Community { get; set; }

    public virtual User Creator { get; set; } = null!;
}
