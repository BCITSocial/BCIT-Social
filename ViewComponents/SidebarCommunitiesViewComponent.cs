using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BCIT_Social.ViewComponents;

// Lists the signed-in user's communities under the sidebar navigation on every page.
public class SidebarCommunitiesViewComponent : ViewComponent
{
    private const int CommunitiesShown = 5;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SidebarCommunitiesViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        var communities = userId is null
            ? []
            : await _context.YourCommunitiesAsync(userId, CommunitiesShown);

        // A community page sets ActiveCommunityId when the viewer is a member. Make sure that community
        // is in the list (it may be older than the five shown) so the sidebar can highlight it.
        if (userId is not null && ViewData["ActiveCommunityId"] is int activeId
            && communities.All(community => community.Id != activeId))
        {
            var active = (await _context.YourCommunitiesAsync(userId, int.MaxValue))
                .FirstOrDefault(community => community.Id == activeId);
            if (active is not null)
            {
                communities.Add(active);
            }
        }

        return View(communities);
    }
}
