
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.NW;

public class CommentController : Controller
{
    private readonly NorthwindContext _context;

    public CommentController(NorthwindContext context)
    {
        _context = context;
    }

    // GET: COMMENTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Comments.ToListAsync());
    }

    // GET: COMMENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(m => m.Id == id);
        if (comment == null)
        {
            return NotFound();
        }

        return View(comment);
    }

    // GET: COMMENTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: COMMENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,PostId,AuthorId,ParentCommentId,Content,CreatedAt,Author,InverseParentComment,ParentComment,Post")] Comment comment)
    {
        if (ModelState.IsValid)
        {
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(comment);
    }

    // GET: COMMENTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        return View(comment);
    }

    // POST: COMMENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,PostId,AuthorId,ParentCommentId,Content,CreatedAt,Author,InverseParentComment,ParentComment,Post")] Comment comment)
    {
        if (id != comment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
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
        return View(comment);
    }

    // GET: COMMENTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(m => m.Id == id);
        if (comment == null)
        {
            return NotFound();
        }

        return View(comment);
    }

    // POST: COMMENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CommentExists(int? id)
    {
        return _context.Comments.Any(e => e.Id == id);
    }
}
