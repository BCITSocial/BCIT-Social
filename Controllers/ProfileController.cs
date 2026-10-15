
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

    /// <summary>
    /// Added userManager to scaffolded code for checking user and database connections.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
    public async Task<IActionResult> Edit(string? userid)
    {
        if (userid == null)
        {
            return NotFound();
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? userid, [Bind("UserId,DisplayName,Bio,Program,Campus,User")] Profile profile)
    {
        if (userid != profile.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(profile);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(profile.UserId))
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
    public async Task<IActionResult> Delete(string? userid)
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

    // POST: PROFILES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? userid)
    {
        var profile = await _context.Profiles.FindAsync(userid);
        if (profile != null)
        {
            _context.Profiles.Remove(profile);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProfileExists(string? userid)
    {
        return _context.Profiles.Any(e => e.UserId == userid);
    }
}
