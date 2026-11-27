using BCIT_Social.Data;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BCIT_Social.ViewComponents;

// The create post modal in the shared layout. Signed-out visitors get nothing, so Create links
// fall through to /Post/Create, which sends them to log in.
public class CreatePostDialogViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CreatePostDialogViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true || userId is null)
        {
            return Content("");
        }

        // PostController.Create stores the post type as one tag, so the choices are the tags.
        var model = new CreatePostViewModel
        {
            // Only show communities the current user is allowed to post to.
            Communities = await _context.PostableCommunities(userId)
                .OrderBy(community => community.Name)
                .Select(community => new SelectListItem { Value = community.Id.ToString(), Text = community.Name })
                .ToListAsync(),
            PostTypes = await _context.Tags
                .AsNoTracking()
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem { Value = tag.Id.ToString(), Text = tag.Name })
                .ToListAsync(),
        };
        return View(model);
    }
}
