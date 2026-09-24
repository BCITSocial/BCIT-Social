using System;
using System.Collections.Generic;

namespace BCIT_Social.NW;

public partial class Bookmark
{
    public int UserId { get; set; }

    public int PostId { get; set; }

    public DateTime? SavedAt { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
