using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Identity;
using BCIT_Social.ViewModels;

public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Added userManager to scaffolded code for checking user and database connections.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    /// <param name="signInManager"></param>
    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: PROFILES
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        var profile = await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (profile == null)
        {
            return View(new ProfilePageViewModel());
        }

        return View(await BuildProfilePageViewModel(profile, userId));
    }

    private async Task<ProfilePageViewModel> BuildProfilePageViewModel(Profile profile, string? viewerId)
    {
        var profileUserId = profile.UserId;
        var posts = await ProjectProfileCards(
                _context.Posts
                    .Where(post => post.AuthorId == profileUserId && post.DateOfDel == null)
                    .OrderByDescending(post => post.CreatedAt)
                    .ThenByDescending(post => post.Id),
                viewerId)
            .ToListAsync();

        var isOwnProfile = viewerId == profileUserId;
        var friendRelationshipStatus = !string.IsNullOrEmpty(viewerId) && !isOwnProfile
            ? await _context.FriendRelationships
                .AsNoTracking()
                .Where(friendship =>
                    friendship.RelationshipKey == GetRelationshipKey(viewerId, profileUserId))
                .Select(friendship => friendship.Status == "Pending"
                    ? friendship.RequesterId == viewerId ? "PendingSent" : "PendingReceived"
                    : friendship.Status)
                .FirstOrDefaultAsync()
            : null;
        var bookmarkedPosts = isOwnProfile
            ? await ProjectProfileCards(
                    _context.Bookmarks
                        .Where(bookmark => bookmark.UserId == profileUserId && bookmark.Post.DateOfDel == null)
                        .OrderByDescending(bookmark => bookmark.SavedAt)
                        .ThenByDescending(bookmark => bookmark.PostId)
                        .Select(bookmark => bookmark.Post),
                    viewerId)
                .ToListAsync()
            : [];

        var communities = await _context.CommunityMembers
            .AsNoTracking()
            .Where(member => member.UserId == profileUserId)
            .OrderBy(member => member.Community.Name)
            .Select(member => new CommunityLinkViewModel
            {
                Id = member.CommunityId,
                Name = member.Community.Name,
                Type = member.Community.Type,
                MemberCount = member.Community.Members.Count()
            })
            .ToListAsync();

        var friends = isOwnProfile
            ? await _context.FriendRelationships
                .AsNoTracking()
                .Where(friendship =>
                    friendship.Status == "Accepted"
                    && (friendship.RequesterId == profileUserId || friendship.RecipientId == profileUserId)
                    && (friendship.RequesterId == profileUserId
                        ? friendship.Recipient.DateOfDel == null
                        : friendship.Requester.DateOfDel == null))
                .Select(friendship => new FriendListItemViewModel
                {
                    RelationshipId = friendship.Id,
                    UserId = friendship.RequesterId == profileUserId
                        ? friendship.RecipientId
                        : friendship.RequesterId,
                    DisplayName = friendship.RequesterId == profileUserId
                        ? _context.Profiles
                            .Where(friendProfile => friendProfile.UserId == friendship.RecipientId)
                            .Select(friendProfile => friendProfile.DisplayName)
                            .FirstOrDefault() ?? friendship.Recipient.UserName ?? "BCIT member"
                        : _context.Profiles
                            .Where(friendProfile => friendProfile.UserId == friendship.RequesterId)
                            .Select(friendProfile => friendProfile.DisplayName)
                            .FirstOrDefault() ?? friendship.Requester.UserName ?? "BCIT member"
                })
                .OrderBy(friend => friend.DisplayName)
                .ToListAsync()
            : [];

        var incomingFriendRequests = viewerId == profileUserId
            ? await _context.FriendRelationships
                .AsNoTracking()
                .Where(request => request.RecipientId == profileUserId && request.Status == "Pending")
                .OrderBy(request => request.CreatedAt)
                .Select(request => new IncomingFriendRequestViewModel
                {
                    Id = request.Id,
                    RequesterDisplayName = _context.Profiles
                        .Where(requesterProfile => requesterProfile.UserId == request.RequesterId)
                        .Select(requesterProfile => requesterProfile.DisplayName)
                        .FirstOrDefault() ?? request.Requester.UserName ?? "BCIT member",
                    CreatedAt = request.CreatedAt
                })
                .ToListAsync()
            : [];

        return new ProfilePageViewModel
        {
            Profile = profile,
            IsOwnProfile = isOwnProfile,
            Posts = posts,
            Bookmarks = bookmarkedPosts,
            Communities = communities,
            Friends = friends,
            IncomingFriendRequests = incomingFriendRequests,
            FriendCount = friends.Count,
            CanSendFriendRequest = !string.IsNullOrEmpty(viewerId) && !isOwnProfile,
            FriendRelationshipStatus = friendRelationshipStatus
        };
    }

    public IActionResult Friends()
    {
        return RedirectToAction(nameof(Index), new { section = "friends" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFriend(string userId)
    {
        var requesterId = _userManager.GetUserId(User);
        if (requesterId == null)
        {
            return Challenge();
        }

        var recipient = await _userManager.FindByIdAsync(userId);
        if (recipient == null || recipient.DateOfDel != null || recipient.Id == requesterId)
        {
            return NotFound();
        }

        var relationshipKey = GetRelationshipKey(requesterId, recipient.Id);
        var relationship = await _context.FriendRelationships
            .FirstOrDefaultAsync(friendship => friendship.RelationshipKey == relationshipKey);

        if (relationship?.Status == "Accepted")
        {
            TempData["FriendRequestMessage"] = "You are already friends with this person.";
        }
        else if (relationship?.Status == "Pending")
        {
            TempData["FriendRequestMessage"] = relationship.RequesterId == requesterId
                ? "Your friend request is pending."
                : "This person has already sent you a request. Review it in your incoming requests.";
        }
        else
        {
            var now = DateTime.UtcNow;
            if (relationship == null)
            {
                relationship = new FriendRelationship
                {
                    RelationshipKey = relationshipKey,
                    RequesterId = requesterId,
                    RecipientId = recipient.Id,
                    Status = "Pending",
                    CreatedAt = now
                };
                _context.FriendRelationships.Add(relationship);
            }
            else
            {
                relationship.RequesterId = requesterId;
                relationship.RecipientId = recipient.Id;
                relationship.Status = "Pending";
                relationship.CreatedAt = now;
                relationship.UpdatedAt = now;
                relationship.DeactivatedAt = null;
            }

            await _context.SaveChangesAsync();
            TempData["FriendRequestMessage"] = "Friend request sent.";
        }

        return RedirectToAction(nameof(Details), new { userid = recipient.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFriend(int relationshipId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        var friendship = await _context.FriendRelationships
            .FirstOrDefaultAsync(relationship =>
                relationship.Id == relationshipId
                && relationship.Status == "Accepted"
                && (relationship.RequesterId == userId || relationship.RecipientId == userId));

        if (friendship == null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        friendship.Status = "Removed";
        friendship.UpdatedAt = now;
        friendship.DeactivatedAt = now;
        await _context.SaveChangesAsync();

        TempData["FriendListMessage"] = "Friend removed.";
        return RedirectToAction(nameof(Index), new { section = "friends" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendFriendRequest(SendFriendRequestViewModel viewModel)
    {
        var requesterId = _userManager.GetUserId(User);
        if (requesterId == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            TempData["FriendRequestError"] = "Enter a valid email address.";
            return RedirectToAction(nameof(Index));
        }

        var recipient = await _userManager.FindByEmailAsync(viewModel.Email.Trim());
        if (recipient == null || recipient.DateOfDel != null)
        {
            TempData["FriendRequestError"] = "No active account matches that email address.";
            return RedirectToAction(nameof(Index));
        }

        if (recipient.Id == requesterId)
        {
            TempData["FriendRequestError"] = "You cannot send a friend request to yourself.";
            return RedirectToAction(nameof(Index));
        }

        var relationshipKey = GetRelationshipKey(requesterId, recipient.Id);
        var relationship = await _context.FriendRelationships
            .FirstOrDefaultAsync(friendship => friendship.RelationshipKey == relationshipKey);

        if (relationship?.Status == "Accepted")
        {
            TempData["FriendRequestError"] = "You are already friends with this person.";
            return RedirectToAction(nameof(Index));
        }

        if (relationship?.Status == "Pending")
        {
            TempData["FriendRequestError"] = relationship.RequesterId == requesterId
                ? "You have already sent this person a friend request."
                : "This person has already sent you a friend request. Review it in your incoming requests.";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        if (relationship == null)
        {
            relationship = new FriendRelationship
            {
                RelationshipKey = relationshipKey,
                RequesterId = requesterId,
                RecipientId = recipient.Id,
                Status = "Pending",
                CreatedAt = now
            };
            _context.FriendRelationships.Add(relationship);
        }
        else
        {
            relationship.RequesterId = requesterId;
            relationship.RecipientId = recipient.Id;
            relationship.Status = "Pending";
            relationship.CreatedAt = now;
            relationship.UpdatedAt = now;
            relationship.DeactivatedAt = null;
        }

        await _context.SaveChangesAsync();
        TempData["FriendRequestMessage"] = "Friend request sent.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToFriendRequest(int requestId, string decision)
    {
        var recipientId = _userManager.GetUserId(User);
        if (recipientId == null)
        {
            return Challenge();
        }

        if (decision is not ("accept" or "deny"))
        {
            return BadRequest();
        }

        var request = await _context.FriendRelationships
            .FirstOrDefaultAsync(friendship =>
                friendship.Id == requestId
                && friendship.RecipientId == recipientId
                && friendship.Status == "Pending");

        if (request == null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        request.Status = decision == "accept" ? "Accepted" : "Denied";
        request.UpdatedAt = now;
        request.DeactivatedAt = decision == "deny" ? now : null;

        await _context.SaveChangesAsync();
        TempData["FriendRequestMessage"] = decision == "accept"
            ? "Friend request accepted."
            : "Friend request denied.";
        return RedirectToAction(nameof(Index));
    }

    private static string GetRelationshipKey(string firstUserId, string secondUserId)
    {
        return string.CompareOrdinal(firstUserId, secondUserId) < 0
            ? $"{firstUserId}:{secondUserId}"
            : $"{secondUserId}:{firstUserId}";
    }

    private IQueryable<PostCardViewModel> ProjectProfileCards(IQueryable<Post> posts, string? userId)
    {
        return posts.AsNoTracking().Select(post => new PostCardViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            AuthorName = _context.Profiles
                .Where(profile => profile.UserId == post.AuthorId)
                .Select(profile => profile.DisplayName)
                .FirstOrDefault() ?? post.Author.UserName ?? "",
            CommunityName = post.Community != null ? post.Community.Name : null,
            CreatedAt = post.CreatedAt,
            Tags = post.PostTags.Select(postTag => postTag.Tag.Name).ToList(),
            CommentCount = _context.Comments.Count(comment => comment.PostId == post.Id),
            IsAuthor = post.AuthorId == userId,
            IsSaved = _context.Bookmarks.Any(bookmark => bookmark.UserId == userId && bookmark.PostId == post.Id)
        });
    }

    // GET: PROFILES/Details/5
    public async Task<IActionResult> Details(string? id, string? userid)
    {
        var requestedUserId = userid ?? id;
        if (requestedUserId == null)
        {
            return NotFound();
        }
        var profile = await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == requestedUserId);
        if (profile == null)
        {
            return NotFound();
        }
        var viewerId = _userManager.GetUserId(User);
        return View("Index", await BuildProfilePageViewModel(profile, viewerId));
    }

    // GET: PROFILES/Create
    public IActionResult Create()
    {
        return View(new ProfileViewModel());
    }

    // POST: PROFILES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 20, 2026
    /// Description: Creation of the profile (This isn't supposed to be used, profile creation is done through the
    /// account registration process)
    /// </summary>
    /// <param name="viewModel">The view model for the profile</param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProfileViewModel viewModel)
    {
        string? userID = _userManager.GetUserId(User);
        if (userID == null)
        {
            return Challenge();
        }
        var profile = new Profile
        {
            UserId = userID,
            DisplayName = viewModel.DisplayName,
            Bio = viewModel.Bio,
            Program = viewModel.Program,
            Campus = viewModel.Campus,
        };
        if (ModelState.IsValid)
        {
            _context.Add(profile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(viewModel);
    }

    // GET: PROFILES/Edit/5
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Edit existing profile, other users are unable to edit other profiles except their own.
    /// </summary>
    public async Task<IActionResult> Edit(string? userid)
    {
        string? currentUserID = _userManager.GetUserId(User);
        if (userid == null)
        {
            return NotFound();
        }
        if (currentUserID != userid)
        {
            return Forbid();
        }
        var profile = await _context.Profiles.FindAsync(userid);
        if (profile == null)
        {
            return NotFound();
        }
        return View(profile);
    }

    // POST: PROFILES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Edit existing profile.
    /// </summary>
    /// <param name="viewModel">The view model for the profile</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel viewModel)
    {
        string? userID = _userManager.GetUserId(User);
        var profile = await _context.Profiles.FindAsync(userID);
        if (userID == null || profile == null)
        {
            return NotFound();
        }
        profile.DisplayName = viewModel.DisplayName;
        profile.Bio = viewModel.Bio;
        profile.Program = viewModel.Program;
        profile.Campus = viewModel.Campus;
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(profile);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(userID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(profile);
    }

    // GET: PROFILES/Delete/5
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Users can only access their own profiles delete page. Access to others is restricted.
    /// </summary>
    public async Task<IActionResult> Delete(string? userid)
    {
        string? currentUserID = _userManager.GetUserId(User);
        if (userid == null)
        {
            return NotFound();
        }
        if (currentUserID != userid)
        {
            return Forbid();
        }
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }

    // POST: PROFILES/Delete/5
    /// <summary>
    /// Author: Joshua Trepanier
    /// Last Revised: Oct 21, 2026
    /// Description: Delete the existing profile (Also deletes the account from the database).
    /// Signs user out once account is deleted.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? userid)
    {
        string? currentUserID = _userManager.GetUserId(User);
        if (userid == null)
        {
            return NotFound();
        }
        if (currentUserID != userid)
        {
            return Forbid();
        }
        var profile = await _context.Profiles.FindAsync(userid);
        var account = await _context.Users.FindAsync(userid);
        if (profile != null && account != null)
        {
            _context.Profiles.Remove(profile);
            _context.Users.Remove(account);
        }
        await _context.SaveChangesAsync();
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProfileExists(string? userid)
    {
        return _context.Profiles.Any(e => e.UserId == userid);
    }
}
