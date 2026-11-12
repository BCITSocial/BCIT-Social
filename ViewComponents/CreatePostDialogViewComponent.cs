using BCIT_Social.Data;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.ViewComponents;

// The create post modal in the shared layout. Signed-out visitors get nothing, so Create links
// fall through to /Post/Create, which sends them to log in. Inside a community page the layout
// passes that community's id, and the post is made in that community.
public class CreatePostDialogViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CreatePostDialogViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(int? communityId = null)
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
        {
            return Content("");
        }

        // PostController.Create stores the post type as one tag, so the choices are the tags.
        var model = new CreatePostViewModel
        {
            PostTypes = await _context.Tags
                .AsNoTracking()
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem { Value = tag.Id.ToString(), Text = tag.Name })
                .ToListAsync(),
        };

        if (communityId is int id)
        {
            model.CommunityName = await _context.Communities
                .AsNoTracking()
                .Where(community => community.Id == id)
                .Select(community => community.Name)
                .FirstOrDefaultAsync();
            // A community that no longer exists falls back to a normal post.
            model.CommunityId = model.CommunityName is null ? null : id;
        }
        return View(model);
    }
}
