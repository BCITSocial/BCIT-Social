
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Description: Initializes the Post controller with database and user management services
    /// </summary>
    /// <param name="context">Application database context.</param>
    /// <param name="userManager">Provides access to the currently authenticated user.</param>
    public PostController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: POSTS
    public async Task<IActionResult> Index()
    {
        var posts = await _context.Posts
            .Include(post => post.PostTags)
            .ThenInclude(postTag => postTag.Tag)
            .Where(post => post.DateOfDel == null)
            .ToListAsync();

        return View(posts);
    }

    // GET: POSTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(m => m.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    // GET: POSTS/Create
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreatePostViewModel
        {
            PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // POST: POSTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Description: Creates a new post using user-entered data and server-generated values
    /// </summary>
    /// <param name="viewModel">The user-entered post information.</param>
    /// <returns>The created post details page, or the Create view when validation fails.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(CreatePostViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync();

            return View(viewModel);
        }

        string? authorId = _userManager.GetUserId(User);

        if (authorId == null)
        {
            return Challenge();
        }

        var post = new Post
        {
            AuthorId = authorId,
            CommunityId = null,
            Content = viewModel.Content,
            CreatedAt = DateTime.UtcNow,
            DateOfDel = null
        };

        _context.Posts.Add(post);

        await _context.SaveChangesAsync();

        var postTag = new PostTags
        {
            PostId = post.Id,
            TagId = viewModel.TagId
        };

        _context.PostTags.Add(postTag);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new
        {
            id = post.Id
        });
    }

    // GET: POSTS/Edit/5
    /// <summary>
    /// Description: Displays the Edit Post form for a post owned by the current user.
    /// Author: Livjot Mahendru
    /// </summary>
    /// <param name="id">The ID of the post being edited.</param>
    /// <returns>The Edit Post view, or an error result when the post cannot be edited.</returns>
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(post =>
                post.Id == id &&
                post.DateOfDel == null);

        if (post == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);

        if (post.AuthorId != currentUserId)
        {
            return Forbid();
        }

        var postTag = await _context.PostTags
            .FirstOrDefaultAsync(postTag => postTag.PostId == post.Id);

        var viewModel = new EditPostViewModel
        {
            Id = post.Id,
            Content = post.Content,
            TagId = postTag?.TagId ?? 0,
            PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // POST: POSTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Description: Updates the user-editable fields of an existing post.
    /// Author: Livjot M
    /// </summary>
    /// <param name="id">The ID of the post being edited.</param>
    /// <param name="viewModel">The updated post information.</param>
    /// <returns>The Post Details page when successful, or the Edit view when validation fails.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int? id, EditPostViewModel viewModel)
    {
        if (id == null || id != viewModel.Id)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(post =>
                post.Id == id &&
                post.DateOfDel == null);

        if (post == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);

        if (post.AuthorId != currentUserId)
        {
            return Forbid();
        }

        bool tagExists = await _context.Tags
            .AnyAsync(tag => tag.Id == viewModel.TagId);

        if (!tagExists)
        {
            ModelState.AddModelError(
                nameof(viewModel.TagId),
                "Please select a valid post type.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync();

            return View(viewModel);
        }

        post.Content = viewModel.Content;

        var existingPostTags = await _context.PostTags
            .Where(postTag => postTag.PostId == post.Id)
            .ToListAsync();

        _context.PostTags.RemoveRange(existingPostTags);

        _context.PostTags.Add(new PostTags
        {
            PostId = post.Id,
            TagId = viewModel.TagId
        });

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new
        {
            id = post.Id
        });
    }

    // GET: POSTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .Include(post => post.Author)
            .FirstOrDefaultAsync(post => post.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    // POST: POSTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PostExists(int? id)
    {
        return _context.Posts.Any(e => e.Id == id);
    }
}
