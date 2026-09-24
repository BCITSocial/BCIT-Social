using System;
using System.Collections.Generic;

namespace project.Models.NW;

public partial class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int AuthorId { get; set; }

    public int? ParentCommentId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual Post Post { get; set; } = null!;
}
