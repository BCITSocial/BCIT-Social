using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Data;

namespace BCIT_Social.Controllers;

public class HomeController : Controller
{
    // The first page shows 20 posts; scrolling loads 10 more at a time.
    private const int FirstPageSize = 20;
    private const int NextPageSize = 10;
    private const int YourCommunitiesShown = 3;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var (posts, hasMore) = await LoadFeedAsync(null, null, FirstPageSize);

        return View(new HomeViewModel
        {
            Posts = posts,
            HasMore = hasMore,
            YourCommunities = userId is null
                ? []
                : await _context.YourCommunitiesAsync(userId, YourCommunitiesShown),
            CurrentUserInitials = await CurrentUserInitialsAsync(userId),
        });
    }

    // Returns the next cards older than the given post, for infinite scroll.
    // beforeTicks and beforeId come from the last card already on the page.
    [HttpGet]
    public async Task<IActionResult> Feed(long beforeTicks, int beforeId)
    {
        var (posts, hasMore) = await LoadFeedAsync(new DateTime(beforeTicks), beforeId, NextPageSize);
        Response.Headers["X-Feed-Has-More"] = hasMore ? "true" : "false";
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
            CurrentUserInitials = await CurrentUserInitialsAsync(userId),
        };

        return Request.Headers.XRequestedWith == "XMLHttpRequest"
            ? PartialView("_PostDetails", model)
            : View(model);
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Description: Displays active posts in the Home feed.
    /// Author: Livjot M
    /// </summary>
    /// <returns>The Home page containing the post feed.</returns>
    public async Task<IActionResult> Index()
    {
        var posts = await _context.Posts
            .AsNoTracking()
            .Where(post => post.DateOfDel == null)
            .OrderByDescending(post => post.CreatedAt)
            .Select(post => new HomePostViewModel
            {
                Id = post.Id,

                AuthorName = _context.Profiles
                    .Where(profile => profile.UserId == post.AuthorId)
                    .Select(profile => profile.DisplayName)
                    .FirstOrDefault()
                    ?? post.Author.UserName
                    ?? "BCIT Social User",

                PostType = post.PostTags
                    .Select(postTag => postTag.Tag.Name)
                    .FirstOrDefault(),

                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt
            })
            .ToListAsync();

        var viewModel = new HomeViewModel
        {
            Posts = posts
        };

        return View(viewModel);
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
        DateTime? beforeCreatedAt, int? beforeId, int take)
    {
        var query = _context.Posts.Where(post => post.DateOfDel == null);

        if (beforeCreatedAt is { } before && beforeId is { } id)
        {
            query = query.Where(post => post.CreatedAt < before
                || (post.CreatedAt == before && post.Id < id));
        }

        var ordered = query
            .OrderByDescending(post => post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Take(take + 1);
        var posts = await ProjectCards(ordered, _userManager.GetUserId(User)).ToListAsync();

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
            CommunityName = post.Community != null ? post.Community.Name : null,
            CreatedAt = post.CreatedAt,
            Tags = post.PostTags.Select(postTag => postTag.Tag.Name).ToList(),
            CommentCount = _context.Comments.Count(comment => comment.PostId == post.Id),
            IsAuthor = userId != null && post.AuthorId == userId,
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
                        .FirstOrDefault() ?? comment.Author.UserName ?? "",
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
