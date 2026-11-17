using System.Security.Claims;
using BCIT_Social.Data;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.ViewComponents;

// The create event popup on the Events page. Signed-out visitors get nothing, so the Create link
// falls through to /Events/Create, which sends them to log in.
public class CreateEventDialogViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CreateEventDialogViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Content("");
        }

        var model = new CreateEventViewModel
        {
            Communities = await _context.CommunityMembers
                .AsNoTracking()
                .Where(member => member.UserId == userId)
                .OrderBy(member => member.Community.Name)
                .Select(member => new SelectListItem
                {
                    Value = member.CommunityId.ToString(),
                    Text = member.Community.Name,
                })
                .ToListAsync(),
            Tags = await _context.Tags
                .AsNoTracking()
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem { Value = tag.Id.ToString(), Text = tag.Name })
                .ToListAsync(),
        };
        return View(model);
    }
}
