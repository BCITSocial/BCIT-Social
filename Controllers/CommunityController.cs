
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;

public class CommunityController : Controller
{
    private readonly ApplicationDbContext _context;

    public CommunityController(ApplicationDbContext context)
    {
        _context = context;
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
        return View();
    }

    // POST: COMMUNITYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CreatorId,Name,Type,Creator,Members")] Community community)
    {
        if (ModelState.IsValid)
        {
            _context.Add(community);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(community);
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
