using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

// Represents an application user account.
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    // Records when the account was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null when active; stores deletion date when the account is soft-deleted.
    public DateTime? DateOfDel { get; set; }
}