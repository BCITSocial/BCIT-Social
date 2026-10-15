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
 * This controller allows authenticated users to create comments on posts.
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
    * @return Redirects to the post details page.
    */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int postId, string content, int? parentCommentId)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest("Comment content is required.");
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
            ParentCommentId = parentCommentId
        });

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Post", new { id = postId });
    }
}
