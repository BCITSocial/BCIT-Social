using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using BCIT_Social.Services;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UploadService _uploadService;

    /// <summary>
    /// Description: Initializes the Post controller with database and user management services
    /// </summary>
    /// <param name="context">Application database context.</param>
    /// <param name="userManager">Provides access to the currently authenticated user.</param>
    public PostController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        UploadService uploadService)
    {
        _context = context;
        _userManager = userManager;
        _uploadService = uploadService;
    }

    // GET: POSTS
    public async Task<IActionResult> Index()
    {
        var posts = await _context.Posts
            .Include(post => post.PostTags)
            .ThenInclude(postTag => postTag.Tag)
            .Include(post => post.Photos)
            .Where(post => post.DateOfDel == null)
            .ToListAsync();

        return View(posts);
    }

    // GET: POSTS/Photo/5
    /// <summary>
    /// Description: Streams a post photo from Blob Storage for display.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="id">The ID of the post photo.</param>
    /// <param name="cancellationToken">A token that can cancel the download operation.</param>
    /// <returns>The photo file, or a not found result if the photo record does not exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Photo(int id, CancellationToken cancellationToken)
    {
        var photo = await _context.PostPhotos
            .FirstOrDefaultAsync(photo => photo.Id == id, cancellationToken);

        if (photo == null)
        {
            return NotFound();
        }

        var image = await _uploadService.DownloadAsync(photo.StorageKey, cancellationToken);
        return File(image.Content, photo.ContentType);
    }

    // GET: POSTS/Details/5
    /// <summary>
    /// Description: Displays the public details of an active post
    /// </summary>
    /// <param name="id">The ID of the post being viewed.</param>
    /// <returns>The Post Details view.</returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .AsNoTracking()
            .Include(post => post.Author)
            .Include(post => post.PostTags)
                .ThenInclude(postTag => postTag.Tag)
            .FirstOrDefaultAsync(post =>
                post.Id == id &&
                post.DateOfDel == null);

        if (post == null)
        {
            return NotFound();
        }

        string? displayName = await _context.Profiles
            .Where(profile => profile.UserId == post.AuthorId)
            .Select(profile => profile.DisplayName)
            .FirstOrDefaultAsync();

        string? currentUserId = _userManager.GetUserId(User);

        var viewModel = new PostDetailsViewModel
        {
            Id = post.Id,
            AuthorName = displayName
                ?? post.Author?.UserName
                ?? "BCIT Social User",

            PostType = post.PostTags
                .FirstOrDefault()?
                .Tag.Name,

            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            CanEdit = currentUserId == post.AuthorId
        };

        return View(viewModel);
    }

    // GET: POSTS/Create
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreatePostViewModel
        {
            PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // POST: POSTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Description: Creates a new post using user-entered data and server-generated values
    /// </summary>
    /// <param name="viewModel">The user-entered post information.</param>
    /// <returns>The created post details page, or the Create view when validation fails.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(
        CreatePostViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var tagIds = viewModel.TagIds.Distinct().ToList();
        if (await _context.Tags.CountAsync(tag => tagIds.Contains(tag.Id), cancellationToken) != tagIds.Count)
        {
            ModelState.AddModelError(nameof(viewModel.TagIds), "Please select valid tags.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync();

            return View(viewModel);
        }

        string? authorId = _userManager.GetUserId(User);

        if (authorId == null)
        {
            return Challenge();
        }

        // Only members can post into a community.
        if (viewModel.CommunityId is int communityId
            && !await _context.CommunityMembers.AnyAsync(
                member => member.CommunityId == communityId && member.UserId == authorId,
                cancellationToken))
        {
            return Forbid();
        }

        var post = new Post
        {
            AuthorId = authorId,
            CommunityId = viewModel.CommunityId,
            Title = viewModel.Title,
            Content = viewModel.Content,
            CreatedAt = DateTime.UtcNow,
            DateOfDel = null
        };

        var uploadedPhotos = new List<UploadedFile>();
        try
        {
            foreach (var photo in viewModel.Photos.Where(photo => photo.Length > 0))
            {
                uploadedPhotos.Add(await _uploadService.UploadAsync(
                    photo,
                    "post",
                    cancellationToken));
            }
        }
        // If an error occurs during upload it deletes any photos already uploaded.
        catch (ArgumentException exception)
        {
            await DeleteUploadedPhotosAsync(uploadedPhotos);
            ModelState.AddModelError(nameof(viewModel.Photos), exception.Message);
            viewModel.PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync();

            return View(viewModel);
        }
        catch
        {
            await DeleteUploadedPhotosAsync(uploadedPhotos);
            throw;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var tagId in viewModel.TagIds.Distinct())
            {
                _context.PostTags.Add(new PostTags
                {
                    PostId = post.Id,
                    TagId = tagId
                });
            }
            await _context.SaveChangesAsync(cancellationToken);

            for (var displayOrder = 0; displayOrder < uploadedPhotos.Count; displayOrder++)
            {
                var photo = uploadedPhotos[displayOrder];
                _context.PostPhotos.Add(new PostPhoto
                {
                    PostId = post.Id,
                    StorageKey = photo.StorageKey,
                    OriginalFileName = photo.OriginalFileName,
                    ContentType = photo.ContentType,
                    FileSizeBytes = photo.FileSizeBytes,
                    DisplayOrder = displayOrder
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            await DeleteUploadedPhotosAsync(uploadedPhotos);
            throw;
        }

        // Back to the feed the post was made from: the community's page, or the home feed.
        return post.CommunityId is int postCommunityId
            ? RedirectToAction("Details", "Community", new { id = postCommunityId })
            : RedirectToAction("Index", "Home");
    }

    // Cleanup: uploaded post photos
    /// <summary>
    /// Description: Deletes uploaded post photos when post creation fails.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="uploadedPhotos">The uploaded photo metadata to clean up.</param>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    private async Task DeleteUploadedPhotosAsync(IEnumerable<UploadedFile> uploadedPhotos)
    {
        foreach (var photo in uploadedPhotos)
        {
            await _uploadService.DeleteAsync(photo.StorageKey, CancellationToken.None);
        }
    }

    // GET: POSTS/Edit/5
    /// <summary>
    /// Description: Displays the Edit Post form for a post owned by the current user.
    /// Author: Livjot Mahendru
    /// </summary>
    /// <param name="id">The ID of the post being edited.</param>
    /// <returns>The Edit Post view, or an error result when the post cannot be edited.</returns>
    [Authorize]
    public async Task<IActionResult> Edit(int? id, string? returnUrl)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(post =>
                post.Id == id &&
                post.DateOfDel == null);

        if (post == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);

        if (post.AuthorId != currentUserId)
        {
            return Forbid();
        }

        var postTagIds = await _context.PostTags
            .Where(postTag => postTag.PostId == post.Id)
            .Select(postTag => postTag.TagId)
            .ToListAsync();

        var viewModel = new EditPostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            TagIds = postTagIds,
            ReturnUrl = EditReturnUrl(returnUrl, post),
            CommunityName = await CommunityNameAsync(post.CommunityId),
            PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // POST: POSTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// Description: Updates the user-editable fields of an existing post.
    /// Author: Livjot M
    /// </summary>
    /// <param name="id">The ID of the post being edited.</param>
    /// <param name="viewModel">The updated post information.</param>
    /// <returns>The Post Details page when successful, or the Edit view when validation fails.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int? id, EditPostViewModel viewModel)
    {
        if (id == null || id != viewModel.Id)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(post =>
                post.Id == id &&
                post.DateOfDel == null);

        if (post == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);

        if (post.AuthorId != currentUserId)
        {
            return Forbid();
        }

        var tagIds = viewModel.TagIds.Distinct().ToList();
        bool tagsExist = await _context.Tags
            .CountAsync(tag => tagIds.Contains(tag.Id)) == tagIds.Count;

        if (!tagsExist)
        {
            ModelState.AddModelError(
                nameof(viewModel.TagIds),
                "Please select valid tags.");
        }

        viewModel.ReturnUrl = EditReturnUrl(viewModel.ReturnUrl, post);

        if (!ModelState.IsValid)
        {
            viewModel.CommunityName = await CommunityNameAsync(post.CommunityId);
            viewModel.PostTypes = await _context.Tags
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToListAsync();

            return View(viewModel);
        }

        post.Title = viewModel.Title;
        post.Content = viewModel.Content;

        var existingPostTags = await _context.PostTags
            .Where(postTag => postTag.PostId == post.Id)
            .ToListAsync();

        _context.PostTags.RemoveRange(existingPostTags);

        foreach (var tagId in tagIds)
        {
            _context.PostTags.Add(new PostTags
            {
                PostId = post.Id,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync();

        return LocalRedirect(viewModel.ReturnUrl);
    }

    // Where the edit page goes back to: the page that opened it when that's on this site,
    // otherwise the post's community page or the home feed.
    private string EditReturnUrl(string? returnUrl, Post post)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return post.CommunityId is int communityId
            ? Url.Action("Details", "Community", new { id = communityId })!
            : Url.Action("Index", "Home")!;
    }

    // The community name shown on the edit page, or null for a post on the main feed.
    private Task<string?> CommunityNameAsync(int? communityId) =>
        communityId is null
            ? Task.FromResult<string?>(null)
            : _context.Communities
                .Where(community => community.Id == communityId)
                .Select(community => community.Name)
                .FirstOrDefaultAsync();

    // GET: POSTS/Delete/5
    /// <summary>
    /// Description: Displays the delete confirmation page for a post owned by the current user.
    /// Last Reviewed: October 22, 2026
    /// </summary>
    /// <param name="id">The ID of the post being deleted.</param>
    /// <returns>The delete confirmation view.</returns>
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(post =>
                post.Id == id &&
                post.DateOfDel == null);

        if (post == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);

        if (post.AuthorId != currentUserId)
        {
            return Forbid();
        }

        return View(post);
    }

    // POST: POSTS/Delete/5
    /// <summary>
    /// Description: Soft deletes a post owned by the current user.
    /// Last Reviewed: October 22, 2026
    /// </summary>
    /// <param name="id">The ID of the post being deleted.</param>
    /// <returns>The Home page after the post is deleted.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts.FirstOrDefaultAsync(item =>
            item.Id == id && item.DateOfDel == null);
        if (post == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);
        if (post.AuthorId != currentUserId)
        {
            return Forbid();
        }

        // Keep related comments and bookmarks intact while hiding the post from active feeds.
        post.DateOfDel = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }
}
