#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.NW;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

public class ProfileController : Controller
{
    private readonly NorthwindContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public ProfileController(NorthwindContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public string Name { get; set; }

    private async Task LoadAsync(IdentityUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);

        Name = userName;
    }

    // GET: Profile
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return View(user);
    }

}
