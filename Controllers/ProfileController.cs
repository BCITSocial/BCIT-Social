using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Identity;
using BCIT_Social.ViewModels;

public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Added userManager to scaffolded code for checking user and database connections.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    /// <param name="signInManager"></param>
    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: PROFILES
    public async Task<IActionResult> Index()
    {
        return View(await _context.Profiles.ToListAsync());
    }

    // GET: PROFILES/Details/5
    public async Task<IActionResult> Details(string? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (profile == null)
        {
            return NotFound();
        }
        return View(profile);
    }

    // GET: PROFILES/Create
    public IActionResult Create()
    {
        return View(new ProfileViewModel());
    }

    // POST: PROFILES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 20, 2026
    /// Description: Creation of the profile (This isn't supposed to be used, profile creation is done through the
    /// account registration process)
    /// </summary>
    /// <param name="viewModel">The view model for the profile</param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProfileViewModel viewModel)
    {
        string? userID = _userManager.GetUserId(User);
        if (userID == null)
        {
            return Challenge();
        }
        var profile = new Profile
        {
            UserId = userID,
            DisplayName = viewModel.DisplayName,
            Bio = viewModel.Bio,
            Program = viewModel.Program,
            Campus = viewModel.Campus,
        };
        if (ModelState.IsValid)
        {
            _context.Add(profile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(viewModel);
    }

    // GET: PROFILES/Edit/5
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Edit existing profile, other users are unable to edit other profiles except their own.
    /// </summary>
    public async Task<IActionResult> Edit(string? userid)
    {
        string? currentUserID = _userManager.GetUserId(User);
        if (userid == null)
        {
            return NotFound();
        }
        if (currentUserID != userid)
        {
            return Forbid();
        }
        var profile = await _context.Profiles.FindAsync(userid);
        if (profile == null)
        {
            return NotFound();
        }
        return View(profile);
    }

    // POST: PROFILES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Edit existing profile.
    /// </summary>
    /// <param name="viewModel">The view model for the profile</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel viewModel)
    {
        string? userID = _userManager.GetUserId(User);
        var profile = await _context.Profiles.FindAsync(userID);
        if (userID == null || profile == null)
        {
            return NotFound();
        }
        profile.DisplayName = viewModel.DisplayName;
        profile.Bio = viewModel.Bio;
        profile.Program = viewModel.Program;
        profile.Campus = viewModel.Campus;
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(profile);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(userID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(profile);
    }

    // GET: PROFILES/Delete/5
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Users can only access their own profiles delete page. Access to others is restricted.
    /// </summary>
    public async Task<IActionResult> Delete(string? userid)
    {
        string? currentUserID = _userManager.GetUserId(User);
        if (userid == null)
        {
            return NotFound();
        }
        if (currentUserID != userid)
        {
            return Forbid();
        }
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }

    // POST: PROFILES/Delete/5
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Delete the existing profile (Also deletes the account from the database).
    /// Signs user out once account is deleted.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? userid)
    {
        string? currentUserID = _userManager.GetUserId(User);
        if (userid == null)
        {
            return NotFound();
        }
        if (currentUserID != userid)
        {
            return Forbid();
        }
        var profile = await _context.Profiles.FindAsync(userid);
        var account = await _context.Users.FindAsync(userid);
        if (profile != null && account != null)
        {
            _context.Profiles.Remove(profile);
            _context.Users.Remove(account);
        }
        await _context.SaveChangesAsync();
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProfileExists(string? userid)
    {
        return _context.Profiles.Any(e => e.UserId == userid);
    }
}
