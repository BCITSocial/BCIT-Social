using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.ViewComponents;

/// <summary>
/// Description: Loads the signed-in user's profile photo for the shared header.
/// Author: Justin Chik
/// </summary>
public class ProfileAvatarViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileAvatarViewComponent(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Description: Finds the signed-in user's profile for the account menu button.
    /// Author: Justin Chik
    /// </summary>
    /// <returns>The header avatar view with the profile photo or fallback icon.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        var profile = userId is null
            ? null
            : await _context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.UserId == userId);

        return View(profile);
    }
}
