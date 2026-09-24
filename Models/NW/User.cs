using System;
using System.Collections.Generic;

namespace project.Models.NW;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Community> Communities { get; set; } = new List<Community>();

    public virtual ICollection<CommunityMember> CommunityMembers { get; set; } = new List<CommunityMember>();

    public virtual ICollection<CommunityMessage> CommunityMessages { get; set; } = new List<CommunityMessage>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Profile? Profile { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
