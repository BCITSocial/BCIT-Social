using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Controllers;

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
