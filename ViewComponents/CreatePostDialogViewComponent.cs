using BCIT_Social.Data;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        return View(model);
    }
}
