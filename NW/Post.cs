using System;
using System.Collections.Generic;

namespace BCIT_Social.NW;

public partial class Post
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public int? CommunityId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? DateOfDel { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Community? Community { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
