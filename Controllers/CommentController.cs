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
 * Controller for managing comments on posts.
 * This controller allows authenticated users to create and delete their own comments on posts.
 */
[Authorize]
public class CommentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /**
    * Create a new comment for the specified post.
    * If a parent comment ID is provided, the new comment will be a reply to that comment.
    * @param postId The ID of the post to comment on.
    * @param content The content of the comment.
    * @param parentCommentId (Optional) The ID of the parent comment if this is a reply.
    * @param returnUrl (Optional) A local URL to return to instead of the post details page.
    * @return Redirects to returnUrl when it is local, otherwise to the post details page.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int postId, string content, int? parentCommentId, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest("Comment content is required.");
        }

        // Apply the same character limit to comments and replies.
        if (content.Length > 1000)
        {
            return BadRequest("Comments must be at most 1000 characters.");
        }

        var postExists = await _context.Posts.AnyAsync(post => post.Id == postId);
        if (!postExists)
        {
            return NotFound();
        }

        if (parentCommentId.HasValue)
        {
            var parentBelongsToPost = await _context.Comments.AnyAsync(comment =>
                comment.Id == parentCommentId.Value && comment.PostId == postId);

            if (!parentBelongsToPost)
            {
                return BadRequest("The parent comment must belong to the selected post.");
            }
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        _context.Comments.Add(new Comments
        {
            AuthorId = userId,
            PostId = postId,
            Content = content.Trim(),
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Post", new { id = postId });
    }

    /**
     * Deletes a comment owned by the current user while preserving its replies.
     * @param id The ID of the comment to delete.
     * @param returnUrl (Optional) A local URL to return to after deletion.
     * @return Redirects to the original page or the post details page.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var comment = await _context.Comments
            .Include(item => item.Replies)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (comment is null) return NotFound();
        if (comment.AuthorId != userId) return Forbid();

        // Move replies up one level so deleting a comment never deletes other users' replies.
        foreach (var reply in comment.Replies.ToList())
            reply.ParentCommentId = comment.ParentCommentId;

        // Update the tracked reply relationships before removing their former parent.
        _context.ChangeTracker.DetectChanges();
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Post", "Home", new { id = comment.PostId });
    }
}
