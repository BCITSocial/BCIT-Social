using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Services;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Identity;
using BCIT_Social.ViewModels;
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UploadService _uploadService;

    /// <summary>
    /// Added userManager to scaffolded code for checking user and database connections.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    /// <param name="signInManager"></param>
    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, UploadService uploadService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _uploadService = uploadService;
    }

    // GET: PROFILES
    public async Task<IActionResult> Index(string? friendSearch = null)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        var profile = await _context.Profiles
            .AsNoTracking()
            .Where(item => item.User.DateOfDel == null)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (profile == null)
        {
            return View(new ProfilePageViewModel());
        }

        return View(await BuildProfilePageViewModel(profile, userId, friendSearch));
    }

    private async Task<ProfilePageViewModel> BuildProfilePageViewModel(Profile profile, string? viewerId, string? friendSearch = null)
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
            .Where(m => m.Community.DateOfDel == null)
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

        var search = isOwnProfile ? friendSearch?.Trim() ?? "" : "";
        var searchResults = new List<FriendSearchResultViewModel>();
        if (isOwnProfile && search.Length > 0)
        {
            var normalizedSearch = search.ToUpperInvariant();
            searchResults = await _context.Profiles.AsNoTracking()
                .Where(p => p.UserId != profileUserId && p.User.DateOfDel == null && p.DisplayName.ToUpper().Contains(normalizedSearch))
                .OrderBy(p => p.DisplayName).ThenBy(p => p.UserId)
                .Select(p => new FriendSearchResultViewModel
                {
                    UserId = p.UserId,
                    DisplayName = p.DisplayName,
                    Program = p.Program,
                    Campus = p.Campus,
                    RelationshipStatus = _context.FriendRelationships
                        .Where(r => (r.RequesterId == profileUserId && r.RecipientId == p.UserId) ||
                                    (r.RecipientId == profileUserId && r.RequesterId == p.UserId))
                        .Select(r => r.Status == "Pending" ? r.RequesterId == profileUserId ? "PendingSent" : "PendingReceived" : r.Status)
                        .FirstOrDefault()
                }).ToListAsync();
        }

        return new ProfilePageViewModel
        {
            Profile = profile,
            IsOwnProfile = isOwnProfile,
            Posts = posts,
            Bookmarks = bookmarkedPosts,
            Communities = communities,
            Friends = friends,
            IncomingFriendRequests = incomingFriendRequests,
            FriendSearch = search,
            FriendSearchResults = searchResults,
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
    public async Task<IActionResult> AddFriend(string userId, string? friendSearch = null, string? section = null, bool fromFriends = false, string? q = null)
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

        if (fromFriends)
        {
            return RedirectToAction("Index", "Friends", new { q });
        }

        return friendSearch is not null
            ? RedirectToAction(nameof(Index), new { section, friendSearch })
            : RedirectToAction(nameof(Details), new { userid = recipient.Id });
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
    public async Task<IActionResult> CancelFriendRequest(int relationshipId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        var now = DateTime.UtcNow;
        var updated = await _context.FriendRelationships
            .Where(request => request.Id == relationshipId
                && request.RequesterId == userId && request.Status == "Pending")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(request => request.Status, "Cancelled")
                .SetProperty(request => request.UpdatedAt, now)
                .SetProperty(request => request.DeactivatedAt, now));

        if (updated == 0)
        {
            return NotFound();
        }

        TempData["FriendRequestMessage"] = "Friend request cancelled.";
        return RedirectToAction("Index", "Friends");
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
            AuthorHasProfilePhoto = _context.Profiles
                .Any(profile => profile.UserId == post.AuthorId && profile.ProfilePhotoStorageKey != null),
            AuthorProfilePhotoOriginalFileName = _context.Profiles
                .Where(profile => profile.UserId == post.AuthorId)
                .Select(profile => profile.ProfilePhotoOriginalFileName)
                .FirstOrDefault(),
            Photos = post.Photos.OrderBy(photo => photo.DisplayOrder).ToList(),
            CommunityName = post.Community != null ? post.Community.Name : null,
            CreatedAt = post.CreatedAt,
            Tags = post.PostTags.Select(postTag => postTag.Tag.Name).ToList(),
            CommentCount = _context.Comments.Count(comment => comment.PostId == post.Id),
            IsAuthor = post.AuthorId == userId,
            AuthorId = post.AuthorId,
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
            .Where(m => m.UserId == requestedUserId)
            .Include(m => m.User)
            .Where(m => m.User.DateOfDel == null)
            .FirstOrDefaultAsync();
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
    public async Task<IActionResult> Create(ProfileViewModel viewModel, CancellationToken cancellationToken)
    {
        string? userID = _userManager.GetUserId(User);
        if (userID == null)
        {
            return Challenge();
        }
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var profile = new Profile
        {
            UserId = userID,
            DisplayName = viewModel.DisplayName,
            Bio = viewModel.Bio,
            Program = viewModel.Program,
            Campus = viewModel.Campus
        };

        UploadedFile? uploadedPhoto = null;
        try
        {
            if (viewModel.ProfilePhoto is { Length: > 0 })
            {
                uploadedPhoto = await _uploadService.UploadAsync(
                    viewModel.ProfilePhoto,
                    "profile",
                    cancellationToken);
                SetProfilePhoto(profile, uploadedPhoto);
            }

            _context.Add(profile);
            await _context.SaveChangesAsync();
        }
        catch
        {
            if (uploadedPhoto != null)
            {
                await _uploadService.DeleteAsync(uploadedPhoto.StorageKey, CancellationToken.None);
            }
            ModelState.AddModelError(nameof(viewModel.ProfilePhoto), "An error occurred while uploading the profile photo.");
            return View(viewModel);
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: PROFILES/Photo/user-id
    /// <summary>
    /// Description: Streams a profile photo from Blob Storage for display.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="id">The ID of the profile owner.</param>
    /// <param name="cancellationToken">A token that can cancel the download operation.</param>
    /// <returns>The profile photo file, or a not found result if no photo exists.</returns>
    [HttpGet]
    public async Task<IActionResult> Photo(string? id, CancellationToken cancellationToken)
    {
        var profile = await _context.Profiles
        .AsNoTracking()
        .FirstOrDefaultAsync(profile => profile.UserId == id, cancellationToken);

        if (string.IsNullOrWhiteSpace(id) || 
            profile?.ProfilePhotoStorageKey == null || 
            profile.ProfilePhotoContentType == null)
        {
            return NotFound();
        }

        var image = await _uploadService.DownloadAsync(profile.ProfilePhotoStorageKey, cancellationToken);
        return File(image.Content, profile.ProfilePhotoContentType);
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
    public async Task<IActionResult> Edit(ProfileViewModel viewModel, CancellationToken cancellationToken)
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

        if (!ModelState.IsValid)
        {
            return View(profile);
        }

        UploadedFile? uploadedPhoto = null;
        var previousPhotoStorageKey = profile.ProfilePhotoStorageKey;
        try
        {
            if (viewModel.ProfilePhoto is { Length: > 0 })
            {
                uploadedPhoto = await _uploadService.UploadAsync(
                    viewModel.ProfilePhoto,
                    "profile",
                    cancellationToken);
                SetProfilePhoto(profile, uploadedPhoto);
            }

            _context.Update(profile);
            await _context.SaveChangesAsync();
        }
        catch (ArgumentException exception)
        {
            if (uploadedPhoto != null)
            {
                await _uploadService.DeleteAsync(uploadedPhoto.StorageKey, CancellationToken.None);
            }

            ModelState.AddModelError(nameof(viewModel.ProfilePhoto), exception.Message);
            return View(profile);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (uploadedPhoto != null)
            {
                await _uploadService.DeleteAsync(uploadedPhoto.StorageKey, CancellationToken.None);
            }

            if (!ProfileExists(userID))
            {
                return NotFound();
            }

            throw;
        }
        catch
        {
            if (uploadedPhoto != null)
            {
                await _uploadService.DeleteAsync(uploadedPhoto.StorageKey, CancellationToken.None);
            }

            throw;
        }

        if (uploadedPhoto != null && previousPhotoStorageKey != null)
        {
            await _uploadService.DeleteAsync(previousPhotoStorageKey, cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Description: Copies uploaded photo metadata to a profile.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="profile">The profile receiving the photo metadata.</param>
    /// <param name="uploadedPhoto">The metadata returned by Blob Storage upload.</param>
    private static void SetProfilePhoto(Profile profile, UploadedFile uploadedPhoto)
    {
        profile.ProfilePhotoStorageKey = uploadedPhoto.StorageKey;
        profile.ProfilePhotoOriginalFileName = uploadedPhoto.OriginalFileName;
        profile.ProfilePhotoContentType = uploadedPhoto.ContentType;
        profile.ProfilePhotoFileSizeBytes = uploadedPhoto.FileSizeBytes;
        profile.ProfilePhotoUpdatedAt = DateTime.UtcNow;
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
    /// Author: Joshua Trepanier, Tin Trinh
    /// Last Revised: Nov 18, 2026
    /// Description: Delete the existing profile (Also deletes the account from the database).
    /// Signs user out once account is deleted.
    /// Checks if user does not own any communities first
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
        var communities = await _context.Communities
                            .Where(c => c.CreatorId == currentUserID)
                            .Where(c => c.DateOfDel == null)
                            .ToListAsync();
        if (communities.Count > 0)
        {
            return RedirectToAction(nameof(CommunityController.CommunityHandling), "Community");
        }
        
        if (profile != null && account != null && communities.Count == 0)
        {
            account.DateOfDel = DateTime.Now.AddDays(7);
            _context.Users.Update(account);
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
