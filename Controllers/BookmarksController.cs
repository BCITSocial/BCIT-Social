using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Controllers;

/**
 * @author: Asher Drybrough
 */
 
/**
 * Controller for managing bookmarks for posts.
 * This controller allows authenticated users to save and remove bookmarks for posts.
 */
[Authorize]
public class BookmarksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookmarksController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /**
    * Save a bookmark for the current user and the specified post.
    * If the bookmark already exists, it will not be added again.
    * @param postId The ID of the post to bookmark.
    * @param returnUrl (Optional) A local URL to return to instead of the post details page.
    * @return Redirects to returnUrl when it is local, otherwise to the post details page.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int postId, string? returnUrl = null)
    {
        var postExists = await _context.Posts.AnyAsync(post => post.Id == postId);
        if (!postExists)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var bookmark = await _context.Bookmarks.FindAsync(userId, postId);
        if (bookmark is null)
        {
            _context.Bookmarks.Add(new Bookmarks
            {
                UserId = userId,
                PostId = postId,
                SavedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Post", new { id = postId });
    }

    /**
    * Remove a bookmark for the current user and the specified post.
    * If the bookmark does not exist, no action will be taken.
    * @param postId The ID of the post to remove the bookmark from.
    * @param returnUrl (Optional) A local URL to return to instead of the post details page.
    * @return Redirects to returnUrl when it is local, otherwise to the post details page.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int postId, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var bookmark = await _context.Bookmarks.FindAsync(userId, postId);
        if (bookmark is not null)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Post", new { id = postId });
    }
}
