using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class FriendsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    /// <summary>Shows friends, incoming and sent requests, and optional people-search results.</summary>
    public async Task<IActionResult> Index(string? q)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        // Both directions of pending requests are needed to label search results correctly.
        var relationships = await db.FriendRelationships.AsNoTracking()
            .Where(r => (r.RequesterId == userId || r.RecipientId == userId)
                && (r.Status == "Accepted" || r.Status == "Pending"))
            .ToListAsync();
        var otherIds = relationships
            .Select(r => r.RequesterId == userId ? r.RecipientId : r.RequesterId)
            .ToList();
        var people = await db.Users.AsNoTracking().Where(u => otherIds.Contains(u.Id) && u.DateOfDel == null)
            .Select(u => new { u.Id, u.UserName }).ToListAsync();
        var profiles = await db.Profiles.AsNoTracking().Where(p => otherIds.Contains(p.UserId)).ToListAsync();
        string DisplayNameFor(string id) => profiles.FirstOrDefault(p => p.UserId == id)?.DisplayName
            ?? people.FirstOrDefault(p => p.Id == id)?.UserName ?? "BCIT member";

        ViewData["Friends"] = relationships.Where(r => r.Status == "Accepted")
            .Select(r => new { Relationship = r, OtherId = r.RequesterId == userId ? r.RecipientId : r.RequesterId })
            .Where(x => people.Any(p => p.Id == x.OtherId))
            .Select(x => new FriendListItemViewModel
            {
                RelationshipId = x.Relationship.Id,
                UserId = x.OtherId,
                DisplayName = DisplayNameFor(x.OtherId)
            })
            .OrderBy(x => x.DisplayName).ToList();
        ViewData["IncomingFriendRequests"] = relationships
            .Where(r => r.Status == "Pending" && r.RecipientId == userId && people.Any(p => p.Id == r.RequesterId))
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new IncomingFriendRequestViewModel
            {
                Id = r.Id,
                RequesterDisplayName = DisplayNameFor(r.RequesterId),
                CreatedAt = r.CreatedAt
            }).ToList();
        ViewData["SentFriendRequests"] = relationships
            .Where(r => r.Status == "Pending" && r.RequesterId == userId && people.Any(p => p.Id == r.RecipientId))
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FriendListItemViewModel
            {
                RelationshipId = r.Id,
                UserId = r.RecipientId,
                DisplayName = DisplayNameFor(r.RecipientId)
            }).ToList();
        ViewData["FriendDetails"] = profiles.ToDictionary(p => p.UserId, p => (p.Program, p.Campus));
        q = q?.Trim();
        ViewData["SearchQuery"] = q;
        if (!string.IsNullOrEmpty(q))
        {
            var matches = await db.Users.AsNoTracking()
                .Where(u => u.Id != userId && u.DateOfDel == null &&
                    ((u.Email != null && u.Email.Contains(q)) ||
                     db.Profiles.Any(p => p.UserId == u.Id && p.DisplayName.Contains(q))))
                .Select(u => new { u.Id, u.Email, u.UserName }).ToListAsync();
            var matchIds = matches.Select(u => u.Id).ToList();
            var matchProfiles = await db.Profiles.AsNoTracking().Where(p => matchIds.Contains(p.UserId)).ToListAsync();
            ViewData["SearchResults"] = matches.Select(u =>
            {
                var profile = matchProfiles.FirstOrDefault(p => p.UserId == u.Id);
                var relationship = relationships.FirstOrDefault(r => r.RequesterId == u.Id || r.RecipientId == u.Id);
                return new FriendSearchResultViewModel
                {
                    UserId = u.Id,
                    DisplayName = profile?.DisplayName ?? u.UserName ?? "BCIT member",
                    Email = u.Email ?? "",
                    RelationshipStatus = FriendSearchResultViewModel.GetRelationshipStatus(
                        relationship?.Status, relationship?.RequesterId, userId)
                };
            }).OrderBy(u => u.DisplayName).ToList();
        }
        return View();
    }
}
