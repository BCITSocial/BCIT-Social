using Microsoft.AspNetCore.Identity;

namespace BCIT_Social.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}