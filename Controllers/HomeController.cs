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

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // /Home?filter=friends or /Home?filter=campus&campus=Burnaby narrows the feed. See FeedFilters.
    public async Task<IActionResult> Index(string? filter, string? campus)
    {
        var userId = _userManager.GetUserId(User);
        filter = FeedFilters.Normalize(filter);
        if (filter == FeedFilters.CAMPUS && string.IsNullOrWhiteSpace(campus))
        {
            campus = await CurrentUserCampusAsync(userId);
        }

        var (posts, hasMore) = await LoadFeedAsync(null, null, FIRSTPAGESIZE, filter, campus);

        return View(new HomeViewModel
        {
            Posts = posts,
            HasMore = hasMore,
            Filter = filter,
            Campus = filter == FeedFilters.CAMPUS ? campus : null,
            Campuses = await _context.Profiles
                .Where(profile => profile.Campus != "")
                .Select(profile => profile.Campus)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync(),
            CurrentUserInitials = await CurrentUserInitialsAsync(userId),
        });
    }

    // Returns the next cards older than the given post, for infinite scroll.
    // beforeTicks and beforeId come from the last card already on the page.
    [HttpGet]
    public async Task<IActionResult> Feed(long beforeTicks, int beforeId, string? filter, string? campus)
    {
        var (posts, hasMore) = await LoadFeedAsync(
            new DateTime(beforeTicks), beforeId, NEXTPAGESIZE, FeedFilters.Normalize(filter), campus);
        Response.Headers["X-Feed-Has-More"] = hasMore ? "true" : "false";
        return PartialView("_PostCards", posts);
    }

    // /Home/Post/5: the post with its comments. The feed script requests it with
    // X-Requested-With to show it in a modal; opening the URL directly shows it as a page.
    [HttpGet]
    public async Task<IActionResult> Post(int id)
    {
        var userId = _userManager.GetUserId(User);
        var post = await _context.ProjectCards(_context.Posts.Where(post => post.Id == id && post.DateOfDel == null), userId)
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
        DateTime? beforeCreatedAt, int? beforeId, int take, string filter, string? campus)
    {
        var query = _context.Posts.Where(post => post.DateOfDel == null);
        var userId = _userManager.GetUserId(User);

        // Communities and Friends need a signed-in user; signed out they match nothing.
        switch (filter)
        {
            case FeedFilters.CAMPUS:
                query = query.Where(post => _context.Profiles
                    .Any(profile => profile.UserId == post.AuthorId && profile.Campus == campus));
                break;
            case FeedFilters.COMMUNITIES:
                query = query.Where(post => post.CommunityId != null && _context.CommunityMembers
                    .Any(member => member.UserId == userId && member.CommunityId == post.CommunityId));
                break;
            case FeedFilters.FRIENDS:
                query = query.Where(post => _context.FriendRelationships
                    .Any(friendship => friendship.Status == "Accepted"
                        && ((friendship.RequesterId == userId && friendship.RecipientId == post.AuthorId)
                            || (friendship.RecipientId == userId && friendship.RequesterId == post.AuthorId))));
                break;
        }

        if (beforeCreatedAt is { } before && beforeId is { } id)
        {
            query = query.Where(post => post.CreatedAt < before
                || (post.CreatedAt == before && post.Id < id));
        }

        var ordered = query
            .OrderByDescending(post => post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Take(take + 1);
        var posts = await _context.ProjectCards(ordered, userId).ToListAsync();

        var hasMore = posts.Count > take;
        return (posts.Take(take).ToList(), hasMore);
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

    private async Task<string?> CurrentUserCampusAsync(string? userId)
    {
        return userId is null
            ? null
            : await _context.Profiles
                .Where(profile => profile.UserId == userId)
                .Select(profile => profile.Campus)
                .FirstOrDefaultAsync();
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
}
