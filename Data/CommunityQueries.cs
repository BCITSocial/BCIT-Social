using BCIT_Social.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Data;

// Community lookups shared by the Home page and the sidebar.
public static class CommunityQueries
{
    // Active communities the user owns or has joined, shared by post creation and its dropdown.
    public static IQueryable<BCIT_Social.Models.Community> PostableCommunities(
        this ApplicationDbContext context, string userId)
    {
        return context.Communities.AsNoTracking()
            .Where(community => community.DateOfDel == null &&
                (community.CreatorId == userId || community.Members.Any(member => member.UserId == userId)));
    }

    // Communities the user has joined, most recently joined first, with member counts.
    public static Task<List<CommunityLinkViewModel>> YourCommunitiesAsync(
        this ApplicationDbContext context, string userId, int take)
    {
        return context.CommunityMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Where(m => m.Community.DateOfDel == null)
            .OrderByDescending(member => member.JoinedAt)
            .Take(take)
            .Select(member => new CommunityLinkViewModel
            {
                Id = member.CommunityId,
                Name = member.Community.Name,
                Type = member.Community.Type,
                MemberCount = context.CommunityMembers.Count(other => other.CommunityId == member.CommunityId),
            })
            .ToListAsync();
    }
}
