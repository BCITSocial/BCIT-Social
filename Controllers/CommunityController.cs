
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Authorization;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CommunityController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommunityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: COMMUNITYS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Communities.ToListAsync());
    }

    // GET: COMMUNITYS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var community = await _context.Communities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (community == null)
        {
            return NotFound();
        }

        return View(community);
    }

    // GET: COMMUNITYS/Create
    public IActionResult Create()
    {
        return View(new CreateCommunityViewModel());
    }

    // POST: COMMUNITYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(CreateCommunityViewModel viewModel)
    {
        string? creatorId = _userManager.GetUserId(User);

        if (creatorId == null)
        {
            return Challenge();
        }

        var community = new Community
        {
            CreatorId = creatorId,
            Name = viewModel.Name,
            Type = viewModel.Type,
        };
        if (ModelState.IsValid)
        {
            _context.Add(community);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new
            {
                community.Id
            });
        }
        return View(viewModel);
    }

    // GET: COMMUNITYS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var community = await _context.Communities.FindAsync(id);
        if (community == null)
        {
            return NotFound();
        }
        return View(community);
    }

    // POST: COMMUNITYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,CreatorId,Name,Type,Creator,Members")] Community community)
    {
        if (id != community.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(community);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommunityExists(community.Id))
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
        return View(community);
    }

    // GET: COMMUNITYS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var community = await _context.Communities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (community == null)
        {
            return NotFound();
        }

        return View(community);
    }

    // POST: COMMUNITYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        if (community != null)
        {
            _context.Communities.Remove(community);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CommunityExists(int? id)
    {
        return _context.Communities.Any(e => e.Id == id);
    }
}
