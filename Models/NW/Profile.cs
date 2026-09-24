using System;
using System.Collections.Generic;

namespace project.Models.NW;

public partial class Profile
{
    public int UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? Bio { get; set; }

    public string? Program { get; set; }

    public string? Campus { get; set; }

    public virtual User User { get; set; } = null!;
}
