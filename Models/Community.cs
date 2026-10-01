using System;
using System.Collections.Generic;

namespace BCIT_Social.NW;

public partial class Community
{
    public int Id { get; set; }

    public int CreatorId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public virtual ICollection<CommunityMember> CommunityMembers { get; set; } = new List<CommunityMember>();

    public virtual ICollection<CommunityMessage> CommunityMessages { get; set; } = new List<CommunityMessage>();

    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
