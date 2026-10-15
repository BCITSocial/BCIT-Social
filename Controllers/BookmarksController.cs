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
    * @return Redirects to the post details page.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int postId)
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
                PostId = postId
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Post", new { id = postId });
    }

    /**
    * Remove a bookmark for the current user and the specified post.
    * If the bookmark does not exist, no action will be taken.
    * @param postId The ID of the post to remove the bookmark from.
    * @return Redirects to the post details page.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int postId)
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

        return RedirectToAction("Details", "Post", new { id = postId });
    }
}
