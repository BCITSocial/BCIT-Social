using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;

namespace BCIT_Social.Controllers;

public class HomeController : Controller
{
    // The first page shows 20 posts; scrolling loads 10 more at a time.
    private const int FIRSTPAGESIZE = 20;
    private const int NEXTPAGESIZE = 10;
    private const int YOURCOMMUNITIESSHOWN = 3;
    private const string ALLFILTER = "all";
    private const string FRIENDSFILTER = "friends";
    private const string COMMUNITIESFILTER = "communities";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Description: Displays the Home feed using the selected source and tag filters.
    /// Author: Livjot Mahendru
    /// Last Revised: November 18, 2026
    /// </summary>
    /// <param name="filter">
    /// the selected feed source, supported values are all, friends, and communities.
    /// </param>
    /// <param name="tagId">
    /// The optional ID of the tag used to filter posts.
    /// </param>
    /// <returns>
    /// The Home view containing the filtered posts and Home page data.
    /// </returns>
    public async Task<IActionResult> Index(
        string filter = ALLFILTER,
        int? tagId = null)
    {
        filter = NormalizeFeedFilter(filter);
        tagId = await NormalizeTagIdAsync(tagId);

        string? userId = _userManager.GetUserId(User);

        var (posts, hasMore) = await LoadFeedAsync(
            null,
            null,
            FIRSTPAGESIZE,
            filter,
            tagId,
            userId);

        var availableTags = await _context.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .Select(tag => new TagFilterViewModel
            {
                Id = tag.Id,
                Name = tag.Name
            })
            .ToListAsync();

        var currentUserProfilePhoto = userId == null
            ? null
            : await _context.Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new
            {
                profile.ProfilePhotoStorageKey,
                profile.ProfilePhotoOriginalFileName
            })
            .FirstOrDefaultAsync();

        return View(new HomeViewModel
        {
            Posts = posts,
            HasMore = hasMore,

            YourCommunities = userId is null
                ? []
                : await _context.YourCommunitiesAsync(
                    userId,
                    YOURCOMMUNITIESSHOWN),

            CurrentUserInitials = await CurrentUserInitialsAsync(userId),
            CurrentUserId = userId,
            HasCurrentUserProfilePhoto = !string.IsNullOrWhiteSpace(currentUserProfilePhoto?.ProfilePhotoStorageKey),
            CurrentUserProfilePhotoOriginalFileName = currentUserProfilePhoto?.ProfilePhotoOriginalFileName,
            CurrentFilter = filter,
            CurrentTagId = tagId,
            AvailableTags = availableTags
        });
    }

    /// <summary>
    /// Description: loads the next group of Home feed posts using the selected source and tag filters.
    /// Author: Livjot Mahendru
    /// Last Revised: November 18, 2026
    /// </summary>
    /// <param name="beforeTicks">
    /// the creation time of the last post displayed
    /// </param>
    /// <param name="beforeId">
    /// the ID of the last post currently displayed
    /// </param>
    /// <param name="filter">
    /// The selected feed source supported values are all, friends, and communities.
    /// </param>
    /// <param name="tagId">
    /// The optional ID of the selected tag.
    /// </param>
    /// <returns>
    /// A partial view containing the next group of filtered post cards.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> Feed(
        long beforeTicks,
        int beforeId,
        string filter = ALLFILTER,
        int? tagId = null)
    {
        filter = NormalizeFeedFilter(filter);
        tagId = await NormalizeTagIdAsync(tagId);

        string? userId = _userManager.GetUserId(User);

        var (posts, hasMore) = await LoadFeedAsync(
            new DateTime(beforeTicks),
            beforeId,
            NEXTPAGESIZE,
            filter,
            tagId,
            userId);

        Response.Headers["X-Feed-Has-More"] =
            hasMore ? "true" : "false";

        return PartialView("_PostCards", posts);
    }

    // /Home/Post/5: the post with its comments. The feed script requests it with
    // X-Requested-With to show it in a modal; opening the URL directly shows it as a page.
    [HttpGet]
    public async Task<IActionResult> Post(int id)
    {
        var userId = _userManager.GetUserId(User);
        var post = await ProjectCards(_context.Posts.Where(post => post.Id == id && post.DateOfDel == null), userId)
            .FirstOrDefaultAsync();
        if (post is null)
        {
            return NotFound();
        }

        var postAuthorId = await _context.Posts.Where(item => item.Id == id).Select(item => item.AuthorId).FirstAsync();
        var model = new PostDetailsViewModel
        {
            Post = post,
            Comments = await LoadCommentThreadsAsync(id, postAuthorId),
            CurrentUserId = userId ?? "",
            CurrentUserInitials = await CurrentUserInitialsAsync(userId),
        };

        return Request.Headers.XRequestedWith == "XMLHttpRequest"
            ? PartialView("_PostDetails", model)
            : View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Loads one page of the feed, newest first, skipping deleted posts.
    // Paging continues from the last card's (CreatedAt, Id), so new posts don't shift or repeat cards.
    // TODO: limit to posts in the Home community once the DB team adds it.
    private async Task<(IReadOnlyList<PostCardViewModel> Posts, bool HasMore)> LoadFeedAsync(
    DateTime? beforeCreatedAt,
    int? beforeId,
    int take,
    string filter,
    int? tagId,
    string? userId)
    {
        var query = _context.Posts
            .Where(post => post.DateOfDel == null);

        // tag filtering
        if (tagId.HasValue)
        {
            query = query.Where(post =>
                post.PostTags.Any(postTag =>
                    postTag.TagId == tagId.Value));
        }

        // Friends feed:
        // Only posts authored by users who have an accepted friendship
        // with the currently signed-in user.
        if (filter == FRIENDSFILTER)
        {
            if (userId == null)
            {
                query = query.Where(post => false);
            }
            else
            {
                query = query.Where(post =>
                    _context.FriendRelationships.Any(friendship =>
                        friendship.Status == "Accepted"
                        && friendship.DeactivatedAt == null
                        &&
                        (
                            friendship.RequesterId == userId
                            && friendship.RecipientId == post.AuthorId

                            ||

                            friendship.RecipientId == userId
                            && friendship.RequesterId == post.AuthorId
                        )));
            }
        }

        // Communities feed:
        // Only posts belonging to communities the current user has joined.
        else if (filter == COMMUNITIESFILTER)
        {
            if (userId == null)
            {
                query = query.Where(post => false);
            }
            else
            {
                query = query.Where(post =>
                    post.CommunityId.HasValue
                    && _context.CommunityMembers.Any(member =>
                        member.UserId == userId
                        && member.CommunityId == post.CommunityId.Value));
            }
        }

        if (beforeCreatedAt is { } before && beforeId is { } id)
        {
            query = query.Where(post =>
                post.CreatedAt < before
                || (post.CreatedAt == before && post.Id < id));
        }

        var ordered = query
            .OrderByDescending(post => post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Take(take + 1);

        var posts = await ProjectCards(ordered, userId)
            .ToListAsync();

        var hasMore = posts.Count > take;

        return (posts.Take(take).ToList(), hasMore);
    }

    // Maps posts to card data. Keeps the query's order.
    private IQueryable<PostCardViewModel> ProjectCards(IQueryable<Post> posts, string? userId)
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
            IsAuthor = userId != null && post.AuthorId == userId,
            AuthorId = post.AuthorId,
            IsSaved = userId != null && _context.Bookmarks
                .Any(bookmark => bookmark.UserId == userId && bookmark.PostId == post.Id),
        });
    }

    // Loads every comment on the post and nests replies under their parent, oldest first.
    private async Task<IReadOnlyList<CommentViewModel>> LoadCommentThreadsAsync(int postId, string postAuthorId)
    {
        var rows = await _context.Comments
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .OrderBy(comment => comment.CreatedAt)
            .ThenBy(comment => comment.Id)
            .Select(comment => new
            {
                comment.ParentCommentId,
                View = new CommentViewModel
                {
                    Id = comment.Id,
                    AuthorName = _context.Profiles
                        .Where(profile => profile.UserId == comment.AuthorId)
                        .Select(profile => profile.DisplayName)
                        .FirstOrDefault() ?? (comment.Author != null ? comment.Author.UserName : null) ?? "",
                    AuthorId = comment.AuthorId,
                    IsPostAuthor = comment.AuthorId == postAuthorId,
                    AuthorHasProfilePicture = _context.Profiles
                        .Any(profile => profile.UserId == comment.AuthorId && profile.ProfilePhotoStorageKey != null),
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                },
            })
            .ToListAsync();

        var byId = rows.ToDictionary(row => row.View.Id, row => row.View);
        var topLevel = new List<CommentViewModel>();
        foreach (var row in rows)
        {
            if (row.ParentCommentId is { } parentId && byId.TryGetValue(parentId, out var parent))
            {
                parent.Replies.Add(row.View);
            }
            else
            {
                topLevel.Add(row.View);
            }
        }

        return topLevel;
    }

    private async Task<string> CurrentUserInitialsAsync(string? userId)
    {
        if (userId is null)
        {
            return "";
        }

        var name = await _context.Profiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.DisplayName)
            .FirstOrDefaultAsync() ?? User.Identity?.Name ?? "";
        return DisplayFormat.Initials(name);
    }

    private static string NormalizeFeedFilter(string? filter)
    {
        return filter?.ToLowerInvariant() switch
        {
            FRIENDSFILTER => FRIENDSFILTER,
            COMMUNITIESFILTER => COMMUNITIESFILTER,
            _ => ALLFILTER
        };
    }

    /// <summary>
    /// Description: Validates the requested Home feed tag
    /// Author: Livjot Mahendru
    /// Last Revised: November 18, 2026
    /// </summary>
    /// <param name="tagId">The requested tag ID</param>
    /// <returns>
    /// The tag ID when it exists, otherwise null to rep all tags
    /// </returns>
    private async Task<int?> NormalizeTagIdAsync(int? tagId)
    {
        if (!tagId.HasValue)
        {
            return null;
        }

        bool tagExists = await _context.Tags
            .AnyAsync(tag => tag.Id == tagId.Value);

        return tagExists
            ? tagId
            : null;
    }
}