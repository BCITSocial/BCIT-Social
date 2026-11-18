
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Authorization;
using BCIT_Social.ViewModels;
using BCIT_Social.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CommunityController : Controller
{
    // Author: Jay
    private const string COMMUNITIES_TEXT_FILE = "communities-text.json";
    private const string DETAILS_TEXT_FILE = "community-details-text.json";
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    // Cover images: saved to Azure Blob storage when it's set up (AzureStorage:ConnectionString),
    // otherwise to App_Data/community-banners. Served by the Banner action.
    private const string BANNER_FOLDER = "community-banners";
    private const long MAX_BANNER_BYTES = 15 * 1024 * 1024;
    private static readonly Dictionary<string, string> _bannerExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
    };
    // Most posts a community page loads.
    private const int MAX_POSTS_SHOWN = 50;
    // --------------

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UploadService _uploadService;
    // Author: Jay
    // env used for loading from wwwroot
    // logger used for warnings
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CommunityController> _logger;
    // -------------

    public CommunityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ILogger<CommunityController> logger, UploadService uploadService)
    {
        _context = context;
        _userManager = userManager;
        _uploadService = uploadService;
        // Author: Jay
        _env = env;
        _logger = logger;
        // -------------
    }

    // GET: COMMUNITY
    // Author: Jay
    // Grabs page text, community cards, and popup form.
    // If DB down, then shows empty cards.
    public async Task<IActionResult> Index()
    {
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        IReadOnlyList<CommunityCardViewModel> cards = [];
        try
        {
            cards = await IndexCardsAsync(text);
        }
        catch (Exception ex)
        {
            // Fallback on macOS since I can't connect to the DB!
            _logger.LogWarning(ex, "Could not load communities; showing an empty list.");
        }

        // Gets how many communities the user has made
        string? creatorId = _userManager.GetUserId(User);
        var communitiesByUser = _context.Communities
            .Where(c => c.CreatorId == creatorId && c.DateOfDel == null)
            .ToList();
        ViewBag.UserCommunityCount = communitiesByUser.Count;
        Console.WriteLine(ViewBag.UserCommunityCount);
        return View(new CommunityIndexViewModel
        {
            Text = text,
            Communities = cards,
            CreateForm = BuildCommunityForm(text.CreateForm, text.Options),
            IsSignedIn = _userManager.GetUserId(User) != null,
        });
    }

    // GET: COMMUNITY/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var community = await _context.Communities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (community == null)
        {
            return NotFound();
        }
        if (community.DateOfDel != null)
        {
            return NotFound();
        }

        CommunityDetailsViewModel viewModel = await DetailsViewModelFromCommunity(community);
        return View(viewModel);
    }

    // GET: COMMUNITY/Members/5
    public async Task<IActionResult> Members(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var community = await _context.Communities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        CommunityMembersViewModel viewModel = await MembersViewModelFromCommunity(community);
        return View(viewModel);
    }


    // GET: COMMUNITY/Create
    // Author: Jay
    public IActionResult Create()
    {
        // reads from json files.
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        string? creatorId = _userManager.GetUserId(User);
        var communitiesByUser = _context.Communities
            .Where(c => c.CreatorId == creatorId)
            .Where(c => c.DateOfDel != null)
            .ToList();
        if (communitiesByUser.Count >= 3)
        {
            return RedirectToAction(nameof(Index));
        }
        return View(BuildCommunityForm(text.CreateForm, text.Options));
    }

    // POST: COMMUNITY/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(CommunityViewModel viewModel, CancellationToken cancellationToken)
    {
        string? creatorId = _userManager.GetUserId(User);

        if (creatorId == null)
        {
            return Challenge();
        }
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        if (!ModelState.IsValid)
        {
            return View(BuildCommunityForm(text.CreateForm, text.Options,
                name: viewModel.Name, description: viewModel.Description, type: viewModel.Type));
        }

        var community = new Community
        {
            CreatorId = creatorId,
            Name = viewModel.Name,
            Type = viewModel.Type,
            // Author: Jaspreet Bath
            Description = viewModel.Description
        };
        var communityMember = new CommunityMember
        {
            UserId = creatorId,
            CommunityId = community.Id,
            IsModerator = true,
        };
        var communitiesByUser = _context.Communities
            .Where(c => c.CreatorId == creatorId)
            .Where(c => c.DateOfDel != null)
            .ToList();
        if (communitiesByUser.Count >= 3)
        {
            return RedirectToAction(nameof(Index));
        }
        community.Members.Add(communityMember);

        // Author: Jay
        AddBannerErrors(viewModel.BannerImage, text.CreateForm);
        // -------------

        UploadedFile? uploadedBannerPhoto = null;
        try
        {
            if (viewModel.BannerPhoto is { Length: > 0 })
            {
                uploadedBannerPhoto = await _uploadService.UploadAsync(
                    viewModel.BannerPhoto,
                    "community",
                    cancellationToken);
                community.BannerPhotoStorageKey = uploadedBannerPhoto.StorageKey;
                community.BannerPhotoOriginalFileName = uploadedBannerPhoto.OriginalFileName;
                community.BannerPhotoContentType = uploadedBannerPhoto.ContentType;
                community.BannerPhotoFileSizeBytes = uploadedBannerPhoto.FileSizeBytes;
                community.BannerPhotoUpdatedAt = DateTime.UtcNow;
            }

            // Author: Jay
            await TrySaveBannerAsync(community, viewModel.BannerImage);
            // -------------
            _context.Add(community);
            _context.Add(communityMember);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException exception)
        {
            if (uploadedBannerPhoto != null)
            {
                await _uploadService.DeleteAsync(uploadedBannerPhoto.StorageKey, CancellationToken.None);
            }

            ModelState.AddModelError(nameof(viewModel.BannerPhoto), exception.Message);
            return View(BuildCommunityForm(text.CreateForm, text.Options,
                name: viewModel.Name, description: viewModel.Description, type: viewModel.Type));
        }
        catch
        {
            if (uploadedBannerPhoto != null)
            {
                await _uploadService.DeleteAsync(uploadedBannerPhoto.StorageKey, CancellationToken.None);
            }

            throw;
        }

        return RedirectToAction(nameof(Details), new
        {
            community.Id
        });
    }

    // GET: COMMUNITY/Photo/5
    /// <summary>
    /// Description: Streams a community cover photo from Blob Storage.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="id">The ID of the community.</param>
    /// <param name="cancellationToken">A token that can cancel the download operation.</param>
    /// <returns>The cover photo file, or a not found result if the community has no cover photo.</returns>
    [HttpGet]
    public async Task<IActionResult> Photo(int id, CancellationToken cancellationToken)
    {
        var community = await _context.Communities
            .AsNoTracking()
            .FirstOrDefaultAsync(community => community.Id == id, cancellationToken);

        if (community?.BannerPhotoStorageKey == null || community.BannerPhotoContentType == null)
        {
            return NotFound();
        }

        var image = await _uploadService.DownloadAsync(community.BannerPhotoStorageKey, cancellationToken);
        return File(image.Content, community.BannerPhotoContentType);
    }

    // GET: COMMUNITY/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var community = await _context.Communities.FindAsync(id);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (!CommunityAccessChecker(true, community))
        {
            return Forbid();
        }
        return View(EditViewModelFromCommunity(community));
    }

    // POST: COMMUNITY/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int? id, CommunityViewModel viewModel)
    {
        var community = await _context.Communities.FindAsync(id);

        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (!CommunityAccessChecker(true, community))
        {
            return Forbid();
        }

        // Author: Jay
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        AddBannerErrors(viewModel.BannerImage, text.EditForm);
        // -------------

        if (ModelState.IsValid)
        {
            community.Name = viewModel.Name;
            community.Type = viewModel.Type;
            // Author: Jaspreet Bath
            community.Description = viewModel.Description;
            // Author: Jay - old cover image is removed only after the new one is saved.
            var replacedBannerKey = await TrySaveBannerAsync(community, viewModel.BannerImage);
            try
            {
                _context.Update(community);
                await _context.SaveChangesAsync();
                await DeleteBannerAsync(replacedBannerKey);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommunityExists(community.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new
            {
                community.Id
            });
        }
        // Author: Jay
        // Show the Edit page again with what was typed and the validation errors.
        return View(BuildCommunityForm(text.EditForm, text.Options, community.Id,
            viewModel.Name, viewModel.Description, viewModel.Type, BannerUrlFor(community)));
        // ----------------
    }

    /// Author: Jay
    /// Description: Streams a community's cover image .
    /// (blob storage(cloud), used first, fallback is local App_Data).
    ///
    /// <param name="id">community id</param>
    // GET: COMMUNITY/Banner/5
    [HttpGet]
    public async Task<IActionResult> Banner(int? id)
    {
        var banner = await _context.Communities
            .AsNoTracking()
            .Where(community => community.Id == id)
            .Select(community => new { community.BannerPhotoStorageKey, community.BannerPhotoContentType })
            .FirstOrDefaultAsync();
        if (banner?.BannerPhotoStorageKey is not { } key
            || banner.BannerPhotoContentType is not { } contentType
            || !_bannerExtensions.ContainsKey(contentType))
        {
            return NotFound();
        }

        Response.Headers.XContentTypeOptions = "nosniff";
        // Changes URL (?v=) when new image is uploaded.
        // Makes it so it's cached.
        Response.Headers.CacheControl = "public, max-age=604800";

        var container = BannerContainer();
        if (container != null)
        {
            try
            {
                var download = await container.GetBlobClient(key).DownloadStreamingAsync();
                return File(download.Value.Content, contentType);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == StatusCodes.Status404NotFound)
            {
                // Not in blob storage, so it was saved locally.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read cover image {Key} from blob storage.", key);
            }
        }

        // App_Data/community-banners/<file name>
        var path = LocalBannerPath(key);
        return System.IO.File.Exists(path) ? PhysicalFile(path, contentType) : NotFound();
    }

    /// Author: Jay
    /// Description: Creates a post inside a community from with 
    /// "+ Post" pop-up on its page.
    /// Only members can post.
    ///
    /// <param name="id">community id</param>
    /// <param name="form">title, content and post type</param>
    // POST: COMMUNITY/CreatePost/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CreatePost(int? id, CommunityPostFormViewModel form)
    {
        string? currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null)
        {
            // back to login page of not signed in.
            return Challenge();
        }
        var community = await _context.Communities.FindAsync(id);
        if (community == null)
        {
            return NotFound();
        }
        if (!await MemberExistsAsync(currentUserId, community))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var post = new Post
            {
                AuthorId = currentUserId,
                CommunityId = community.Id,
                Title = form.Title.Trim(),
                Content = form.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
            };
            if (form.TagId is int tagId && await _context.Tags.AnyAsync(tag => tag.Id == tagId))
            {
                post.PostTags.Add(new PostTags { TagId = tagId });
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        // Back to the community, scrolled to its posts.
        return RedirectToAction(nameof(Details), null, new { community.Id }, "posts");
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 19, 2026
    /// Description: GET call to join a community
    /// Redirects to the Join page, checks if the user has not joined this community, otherwise redirects
    /// </summary>
    /// <param name="id">community id</param>
    // GET: COMMUNITY/Join/5
    [Authorize]
    public async Task<IActionResult> Join(int? id)
    {
        string? currentUserId = _userManager.GetUserId(User);
        if (id == null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Forbid();
        }
        var community = await _context.Communities
            .FirstOrDefaultAsync(m => m.Id == id);

        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (await MemberExistsAsync(currentUserId, community))
        {
            return RedirectToAction(nameof(Index));
        }

        return View(community);
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 19, 2026
    /// Description: POST call to join a community
    /// Adds user to a community, otherwise redirects to index.
    /// </summary>
    /// <param name="id">community id</param>
    /// <returns></returns>
    // POST: COMMUNITY/Join/5
    [HttpPost, ActionName("Join")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> JoinConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Forbid();
        }

        if (await MemberExistsAsync(currentUserId, community))
        {
            return RedirectToAction(nameof(Index));
        }

        var communityMember = new CommunityMember
        {
            UserId = currentUserId,
            CommunityId = community.Id,
            IsModerator = false,
        };
        if (ModelState.IsValid)
        {
            community.Members.Add(communityMember);
            _context.Add(communityMember);
            _context.Update(community);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new
            {
                community.Id
            });
        }
        return View(communityMember);
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 21, 2026
    /// Description: GET call to leave a community
    /// Redirects to the Leave page, checks if the user is in this community, otherwise redirects
    /// Also checks if user is Creator, they cannot leave (as of right now)
    /// </summary>
    /// <param name="id">community id</param>
    // GET: COMMUNITY/Leave/5
    [Authorize]
    public async Task<IActionResult> Leave(int? id)
    {
        string? currentUserId = _userManager.GetUserId(User);
        if (id == null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Forbid();
        }
        var community = await _context.Communities
            .Include(m => m.Members)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }

        if (community.CreatorId == currentUserId)
        {
            return RedirectToAction(nameof(LeaveCreator), new
            {
                community.Id
            });
        }

        if (await MemberExistsAsync(currentUserId, community))
        {
            return View(community);
        }
        return RedirectToAction(nameof(Index));

    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 21, 2026
    /// Description: POST call to leave a community
    /// Removes user to a community, otherwise redirects to index.
    /// </summary>
    /// <param name="id">community id</param>
    /// <returns></returns>
    // POST: COMMUNITY/Leave/5
    [HttpPost, ActionName("Leave")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> LeaveConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (await MemberExistsAsync(currentUserId, community) == false)
        {
            return RedirectToAction(nameof(Index));
        }
        var communityMember = await _context.CommunityMembers.FindAsync(currentUserId, id);
        if (communityMember == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            community.Members.Remove(communityMember);
            _context.Remove(communityMember);
            _context.Update(community);

            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 13, 2026
    /// Description: GET call to leave a community as the Creator
    /// Redirects to the LeaveCreator page, checks if the user is in this community, otherwise redirects
    /// Redirects to regular Leave page if just member
    /// </summary>
    /// <param name="id">community id</param>
    // GET: COMMUNITY/LeaveCreator/5
    [Authorize]
    public async Task<IActionResult> LeaveCreator(int? id)
    {
        string? currentUserId = _userManager.GetUserId(User);
        if (id == null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Forbid();
        }
        var community = await _context.Communities
            .Include(m => m.Members)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }

        if (community.CreatorId != currentUserId)
        {
            return RedirectToAction(nameof(Leave));
        }

        if (await MemberExistsAsync(currentUserId, community))
        {
            return View(await CreatorLeaveViewModelFromCommunity(community));
        }
        return RedirectToAction(nameof(Index));

    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 21, 2026
    /// Description: POST call to leave a community
    /// Removes user from a community (they leave), otherwise redirects to index.
    /// Community is either deleted or Creator role is passed to another member.
    /// </summary>
    /// <param name="id">community id</param>
    /// <returns></returns>
    // POST: COMMUNITY/LeaveCreator/5
    [HttpPost, ActionName("LeaveCreatorSolo")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> LeaveCreatorSolo(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (await MemberExistsAsync(currentUserId, community) == false)
        {
            return RedirectToAction(nameof(Index));
        }
        var communityMember = await _context.CommunityMembers.FindAsync(currentUserId, id);
        if (communityMember == null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            community.DateOfDel = DateTime.Now.AddDays(7);
            _context.Communities.Update(community);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 21, 2026
    /// Description: POST call to leave a community
    /// Removes user from a community (they leave), otherwise redirects to index.
    /// Community is either deleted or Creator role is passed to another member.
    /// </summary>
    /// <param name="id">community id</param>
    /// <returns></returns>
    // POST: COMMUNITY/LeaveCreator/5
    [HttpPost, ActionName("LeaveCreatorMulti")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> LeaveCreatorMulti(int? id, CommunitiesLeaveCreatorModel viewModel)
    {
        var community = await _context.Communities.FindAsync(id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (await MemberExistsAsync(currentUserId, community) == false)
        {
            return RedirectToAction(nameof(Index));
        }
        var communityMember = await _context.CommunityMembers.FindAsync(currentUserId, id);
        var newCreator = await _context.CommunityMembers.FindAsync(viewModel.CreatorId, id);

        if (communityMember == null || newCreator == null)
        {
            Console.WriteLine($"\n!{viewModel.CreatorId}!\n");
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            community.CreatorId = viewModel.CreatorId.Trim();
            community.Creator = newCreator.User;
            newCreator.IsModerator = true;

            community.Members.Remove(communityMember);
            _context.CommunityMembers.Remove(communityMember);
            _context.CommunityMembers.Update(newCreator);

            _context.Communities.Update(community);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 11, 2026
    /// Description: GET call to kick a member from a community
    /// Redirects to the Kick page, checks if the user is in this community and if user has rights to kick, otherwise redirects/forbids
    /// </summary>
    /// <param name="id">community id</param>
    /// <param name="userid">user id</param>
    // GET: COMMUNITY/Kick/5/abc
    [Authorize]
    [Route("Community/Kick/{id}/{userid}")]
    public async Task<IActionResult> Kick(int? id, string? userid)
    {
        var communityMemberSearch = _context.CommunityMembers.Include(m => m.User).Where(m => m.UserId == userid && m.CommunityId == id);
        if (communityMemberSearch.IsNullOrEmpty())
            return NotFound();
        var communityMember = await communityMemberSearch.FirstAsync();

        string? currentUserId = _userManager.GetUserId(User);
        var currentCommunityUser = await _context.CommunityMembers.FindAsync(currentUserId, id);
        var community = await _context.Communities.FirstOrDefaultAsync(m => m.Id == id);

        if (id == null || userid == null || communityMember == null || community == null || community.DateOfDel != null)
            return NotFound();
        if (currentUserId == null || currentCommunityUser == null)
            return Forbid();

        // takes back to community page if they aren't a member
        if (currentCommunityUser == null)
        {
            return RedirectToAction(nameof(Details), new
            {
                community.Id
            });
        }
        // user has to be a mod or redirects to members page
        if (!currentCommunityUser.IsModerator)
        {
            return RedirectToAction(nameof(Members), new
            {
                community.Id
            });
        }
        // if the community member is a mod, only the creator can kick
        // BTW these checks are only for precaution, they should not have access via frondend links, only if they type it out
        if (communityMember.IsModerator && currentUserId != community?.CreatorId)
        {
            return RedirectToAction(nameof(Details), new
            {
                community?.Id
            });
        }

        if (await MemberExistsAsync(userid, community))
        {
            var viewModel = await MemberDetailViewModelFromCommunity(communityMember);
            return View(viewModel);
        }
        return RedirectToAction(nameof(Members), new
        {
            community.Id
        });

    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 11, 2026
    /// Description: POST call to kick a member from a community
    /// Removes specified user from a community, otherwise redirects to members.
    /// </summary>
    /// <param name="id">community id</param>
    /// <param name="userid">user id</param>
    /// <returns></returns>
    // POST: COMMUNITY/Kick/5/abc
    [HttpPost, ActionName("Kick")]
    [Route("Community/Kick/{id}/{userid}")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> KickConfirmed(int? id, string? userid, CommunityMemberDetailsViewModel viewModel)
    {
        var community = await _context.Communities.FindAsync(id);
        var communityMember = await _context.CommunityMembers.FindAsync(userid, id);
        string? currentUserId = _userManager.GetUserId(User);

        if (community == null || communityMember == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }
        // checks if community member is even in the community
        if (communityMember.CommunityId != id)
        {
            return RedirectToAction(nameof(Index));
        }
        if (ModelState.IsValid)
        {
            community.Members.Remove(communityMember);
            _context.Remove(communityMember);
            _context.Update(community);

            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Members), new
        {
            community.Id
        });
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 11, 2026
    /// Description: GET call to edit the moderator status of a member from a community
    /// Redirects to the EditStatus page, checks if the user is in this community and if current user has rights to edit mod status, otherwise redirects/forbids
    /// </summary>
    /// <param name="id">community id</param>
    /// <param name="userid">user id</param>
    // GET: COMMUNITY/EditStatus/5/abc
    [Authorize]
    [Route("Community/EditStatus/{id}/{userid}")]
    public async Task<IActionResult> EditStatus(int? id, string? userid)
    {
        var communityMemberSearch = _context.CommunityMembers.Include(m => m.User).Where(m => m.UserId == userid && m.CommunityId == id);
        if (communityMemberSearch.IsNullOrEmpty())
        {
            return NotFound();
        }
        var communityMember = await communityMemberSearch.FirstAsync();

        string? currentUserId = _userManager.GetUserId(User);
        var currentCommunityUser = await _context.CommunityMembers.FindAsync(currentUserId, id);
        var community = await _context.Communities.FirstOrDefaultAsync(m => m.Id == id);

        if (id == null || userid == null || community == null || community.DateOfDel != null)
            return NotFound();
        if (currentUserId == null || currentCommunityUser == null)
            return Forbid();

        // user has to be the creator
        if (!CommunityAccessChecker(true, community))
        {
            return RedirectToAction(nameof(Members), new
            {
                community.Id
            });
        }

        if (await MemberExistsAsync(userid, community))
        {
            return View(await MemberDetailViewModelFromCommunity(communityMember));
        }
        return RedirectToAction(nameof(Members), new
        {
            community.Id
        });

    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 11, 2026
    /// Description: POST call to edit the moderator status of a member from a community
    /// Changes a moderator status of a community member
    /// </summary>
    /// <param name="id">community id</param>
    /// <param name="userid">user id</param>
    /// <returns></returns>
    // POST: COMMUNITY/EditStatus/5/abc
    [HttpPost, ActionName("EditStatus")]
    [Route("Community/EditStatus/{id}/{userid}")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> EditStatusConfirm(int? id, string? userid, CommunityMemberDetailsViewModel viewModel)
    {
        var community = await _context.Communities.FindAsync(id);
        var communityMember = await _context.CommunityMembers.FindAsync(userid, id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || communityMember == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (communityMember.CommunityId != id)
        {
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            communityMember.IsModerator = viewModel.IsModerator;
            _context.Update(communityMember);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Members), new
        {
            community.Id
        });
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 11, 2026
    /// Description: POST call to make another user the creator of a server
    /// Changes a moderator status of a community member
    /// </summary>
    /// <param name="id">community id</param>
    /// <param name="userid">user id</param>
    /// <returns></returns>
    // POST: COMMUNITY/PassCreator/5/abc
    [HttpPost, ActionName("PassCreator")]
    [Route("Community/PassCreator/{id}/{userid}")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> PassCreator(int? id, string? userid)
    {
        var community = await _context.Communities.FindAsync(id);
        var communityMember = await _context.CommunityMembers.FindAsync(userid, id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || communityMember == null || community.DateOfDel != null)
        {
            return NotFound();
        }

        if (currentUserId == null || community.CreatorId != currentUserId)
        {
            return Forbid();
        }

        if (communityMember.CommunityId != id)
        {
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            community.CreatorId = communityMember.UserId;
            communityMember.IsModerator = true;
            _context.Update(communityMember);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Members), new
        {
            community.Id
        });
    }

    // GET: COMMUNITY/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var community = await _context.Communities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        if (!CommunityAccessChecker(true, community))
        {
            return Forbid();
        }

        // Author: Jay
        return View(await DeleteViewModelFromCommunity(community));
    }

    // POST: COMMUNITY/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        if (community == null || community.DateOfDel != null)
        {
            return NotFound();
        }
        string? bannerKey = null;
        if (community != null)
        {
            // Author: Jay - only the creator can delete (the check's result was ignored before).
            if (!CommunityAccessChecker(true, community))
            {
                return Forbid();
            }
            bannerKey = community.BannerPhotoStorageKey;
            community.DateOfDel = DateTime.Now.AddDays(7);
            _context.Communities.Update(community);
            await _context.SaveChangesAsync();
        }
        // Author: Jay
        await DeleteBannerAsync(bannerKey);
        return RedirectToAction(nameof(Index));
    }

    // ---------------------
    // CREATING VIEW MODELS
    // ---------------------

    /// Author: Jay
    /// Description: Creates the cards for the Communities page in one query: 
    ///     each community's member count, 
    ///     whether signed-in user has joined with its tags read from Type.
    private async Task<IReadOnlyList<CommunityCardViewModel>> IndexCardsAsync(CommunitiesText text)
    {
        var currentUserId = _userManager.GetUserId(User);
        // waits for db query.
        var communities = await _context.Communities
            .AsNoTracking()
            .Where(m => m.DateOfDel == null)
            .Select(community => new
            {
                community.Id,
                community.Name,
                community.Type,
                community.Description,
                MemberCount = community.Members.Count,
                IsJoined = currentUserId != null
                    && community.Members.Any(member => member.UserId == currentUserId),
                IsCreator = currentUserId != null && community.CreatorId == currentUserId,
                HasBanner = community.BannerPhotoStorageKey != null,
                community.BannerPhotoUpdatedAt,
            })
            .ToListAsync();

        return communities.Select(community =>
        {
            var tags = CommunityTags.Parse(community.Type, text.Options);
            return new CommunityCardViewModel
            {
                Id = community.Id,
                Name = community.Name,
                Description = string.IsNullOrWhiteSpace(community.Description)
                    ? text.DefaultDescription
                    : community.Description,
                Badges = tags.Badges,
                Location = tags.Campus ?? text.DefaultCampus,
                MemberCount = community.MemberCount,
                IsJoined = community.IsJoined,
                IsCreator = community.IsCreator,
                BannerUrl = community.HasBanner ? BannerUrl(community.Id, community.BannerPhotoUpdatedAt) : null,
                Topics = tags.Topics,
                Status = tags.Status,
            };
        }).ToList();
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 14, 2026
    /// Description: Creates a view model for Community Editing
    /// </summary>
    /// <param name="community">a Community</param>
    /// <returns>a view model</returns>
    private CommunityFormViewModel EditViewModelFromCommunity(Community community)
    {
        // Author: Jay
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        return BuildCommunityForm(text.EditForm, text.Options, community.Id,
            community.Name, community.Description, community.Type, BannerUrlFor(community));
    }


    /// Author: Jay
    /// Description: Creates the community form (create pop-up, Settings pop-up, Create page, Edit page).
    /// Dropdowns preselected from the saved Type.
    /// 
    /// <param name="text">labels for the create or edit form</param>
    /// <param name="options">dropdown choices</param>
    /// <param name="id">community id when editing, else null</param>
    /// <param name="bannerUrl">current cover image when editing, else null</param>
    private static CommunityFormViewModel BuildCommunityForm(CommunityFormText text, CommunityOptions options,
        int? id = null, string? name = null, string? description = null, string? type = null,
        string? bannerUrl = null)
    {
        var tags = CommunityTags.Parse(type, options);
        return new CommunityFormViewModel
        {
            Text = text,
            Options = options,
            Id = id,
            Name = name ?? "",
            Description = description ?? "",
            Category = tags.Category ?? "",
            Tag = tags.Tag ?? "",
            Campus = tags.Campus ?? "",
            BannerUrl = bannerUrl,
            MaxBannerBytes = MAX_BANNER_BYTES,

            Type = string.IsNullOrWhiteSpace(type) ? options.Categories.FirstOrDefault() ?? "" : type,
        };
    }


    /// Author: Jay
    /// Description: Creates a view model for the Delete community page.
    /// 
    /// <param name="community">A Community</param>
    /// <returns>view model</returns>
    private async Task<CommunityDeleteViewModel> DeleteViewModelFromCommunity(Community community)
    {
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        var tags = CommunityTags.Parse(community.Type, text.Options);
        return new CommunityDeleteViewModel
        {
            Text = text.DeletePage,
            Id = community.Id,
            Name = community.Name,
            Badges = tags.Badges,
            Location = tags.Campus ?? text.DefaultCampus,
            MemberCount = await _context.CommunityMembers.CountAsync(member => member.CommunityId == community.Id),
            MembersLabel = text.MembersLabel,
        };
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 19, 2026
    /// Description: Creates a view model for Community Details
    /// </summary>
    /// <param name="community">a Community</param>
    /// <returns>a view model</returns>
    private async Task<CommunityDetailsViewModel> DetailsViewModelFromCommunity(Community community)
    {
        var members = await _context.CommunityMembers
        .Where(m => m.CommunityId == community.Id)
        .Include(u => u.User)
        .ToListAsync();
        var creator = await _userManager.FindByIdAsync(community.CreatorId);
        var creatorName = creator?.UserName;

        var currentUserId = _userManager.GetUserId(User);
        bool isMember = MemberExists(currentUserId, community);
        bool isMod = false;
        if (isMember)
        {
            var communityMember = _context.CommunityMembers.Find(currentUserId, community.Id);
            isMod = communityMember?.IsModerator ?? false;
        }

        // Author: Jay
        var text = LoadText<CommunitiesText>(COMMUNITIES_TEXT_FILE);
        var detailsText = LoadText<CommunityDetailsText>(DETAILS_TEXT_FILE);
        var tags = CommunityTags.Parse(community.Type, text.Options);
        bool isCreator = currentUserId != null && currentUserId == community.CreatorId;
        var bannerUrl = BannerUrlFor(community);

        var posts = await _context.Posts
            .AsNoTracking()
            .Where(post => post.CommunityId == community.Id && post.DateOfDel == null)
            .OrderByDescending(post => post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Take(MAX_POSTS_SHOWN)
            .Select(post => new PostCardViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorName = _context.Profiles
                    .Where(profile => profile.UserId == post.AuthorId)
                    .Select(profile => profile.DisplayName)
                    .FirstOrDefault() ?? post.Author.UserName ?? "",
                CreatedAt = post.CreatedAt,
                Tags = post.PostTags.Select(postTag => postTag.Tag.Name).ToList(),
                CommentCount = _context.Comments.Count(comment => comment.PostId == post.Id),
                IsAuthor = currentUserId != null && post.AuthorId == currentUserId,
            })
            .ToListAsync();

        // Post types for the create post pop-up; only members can post.
        IReadOnlyList<SelectListItem> postTypes = isMember
            ? await _context.Tags
                .AsNoTracking()
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem { Value = tag.Id.ToString(), Text = tag.Name })
                .ToListAsync()
            : [];
        // -------------

        var viewModel = new CommunityDetailsViewModel
        {
            Id = community.Id,
            Name = community.Name,
            Type = community.Type,
            Description = community.Description,
            BannerPhotoStorageKey = community.BannerPhotoStorageKey,
            Creator = creatorName ?? "Not Found",
            Members = members,
            IsMod = isMod,
            IsMember = isMember,
            // Author: Jay
            Text = detailsText,
            About = string.IsNullOrWhiteSpace(community.Description)
                ? text.DefaultDescription
                : community.Description,
            Badges = tags.Badges,
            Location = tags.Campus ?? detailsText.Meta.Campus,
            // Author: Jay
            // Uses their first tag first.
            // "??" fallback to use right side arg.
            Category = tags.Category ?? tags.Topics.FirstOrDefault(),
            IsCreator = isCreator,
            BannerUrl = bannerUrl,
            SettingsForm = isCreator
                ? BuildCommunityForm(text.EditForm, text.Options, community.Id,
                    community.Name, community.Description, community.Type, bannerUrl)
                : null,
            Posts = posts,
            PostTypes = postTypes,
            // -------------
        };
        return viewModel;
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 5, 2026
    /// Description: Creates a view model for Memberlist.
    /// </summary>
    /// <param name="community">a Community</param>
    /// <returns>a view model</returns>
    private async Task<CommunityMembersViewModel> MembersViewModelFromCommunity(Community community)
    {
        var members = await _context.CommunityMembers
        .Where(m => m.CommunityId == community.Id)
        .Include(u => u.User)
        .ToListAsync();
        var currentUserId = _userManager.GetUserId(User);
        bool isMember = MemberExists(currentUserId, community);
        bool isMod = false;
        bool isCreator = currentUserId == community.CreatorId;
        if (isMember)
        {
            var communityMember = _context.CommunityMembers.Find(currentUserId, community.Id);
            isMod = communityMember?.IsModerator ?? false;
        }

        var viewModel = new CommunityMembersViewModel
        {
            Id = community.Id,
            Name = community.Name,
            CreatorId = community.CreatorId,
            Members = members,
            IsMod = isMod,
            IsCreator = isCreator
        };
        return viewModel;
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 11, 2026
    /// Description: Creates a view model for a Member.
    /// </summary>
    /// <param name="communityMember">a Community Member</param>
    /// <returns>a view model</returns>
    private async Task<CommunityMemberDetailsViewModel> MemberDetailViewModelFromCommunity(CommunityMember communityMember)
    {
        // TODO: replace Username with displayname when db is updated
        var profile = await _context.Profiles.FindAsync(communityMember.UserId);
        string displayname = profile?.DisplayName ?? communityMember.User.UserName ?? "No Name Found";

        var viewModel = new CommunityMemberDetailsViewModel
        {
            Id = communityMember.UserId,
            DisplayName = displayname,
            IsModerator = communityMember.IsModerator,
            CommunityName = communityMember.Community.Name,
            CommunityId = communityMember.CommunityId
        };
        return viewModel;
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 13, 2026
    /// Description: Creates a view model for a Member.
    /// </summary>
    /// <param name="communityMember">a Community Member</param>
    /// <returns>a view model</returns>
    private async Task<CommunitiesLeaveCreatorModel> CreatorLeaveViewModelFromCommunity(Community community)
    {
        var viewModel = new CommunitiesLeaveCreatorModel
        {
            Id = community.Id,
            CommunityName = community.Name,
            CreatorId = community.CreatorId,
            MemberCount = community.Members.Count,
            Members = community.Members
        };
        return viewModel;
    }

    // -------
    // HELPERS
    // -------


    /// Author: Jay
    /// Description: Reads page text from a JSON file from wwwroot/content
    /// Missing, empty or broken file gives the default texts in text class.
    /// 
    /// <param name="fileName">file name inside wwwroot/content</param>
    /// <returns>the page text</returns>
    private T LoadText<T>(string fileName) where T : new()
    {
        var path = Path.Combine(_env.WebRootPath, "content", fileName);
        if (!System.IO.File.Exists(path))
        {
            return new T();
        }

        try
        {
            var json = System.IO.File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? new T()
                : JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not read {Path}; using the default text.", path);
            return new T();
        }
    }


    // ---------------------
    // COVER IMAGE HELPERS
    // Author: Jay
    // ---------------------

    // URL of the Banner action; ?v= changes after each upload so browsers fetch the new image.
    private string? BannerUrl(int id, DateTime? updatedAt) =>
        Url.Action(nameof(Banner), new { id, v = updatedAt?.Ticks ?? 0 });

    private string? BannerUrlFor(Community community) =>
        community.BannerPhotoStorageKey == null ? null : BannerUrl(community.Id, community.BannerPhotoUpdatedAt);

    // Adds a form error when the picked cover image is too big or not an image.
    private void AddBannerErrors(IFormFile? file, CommunityFormText text)
    {
        // No image picked: nothing to check, the cover is optional.
        if (file == null || file.Length == 0)
        {
            return;
        }
        // Too big: fill {size} in the message with the real limit, e.g. "{size} MB".
        if (file.Length > MAX_BANNER_BYTES)
        {
            ModelState.AddModelError(nameof(CommunityViewModel.BannerImage),
                text.CoverTooLarge.Replace("{size}", CommunityFormViewModel.FormatSize(MAX_BANNER_BYTES)));
        }
        // Wrong type, or a renamed file that isn't really an image.
        else if (!_bannerExtensions.ContainsKey(file.ContentType) || !HasImageSignature(file))
        {
            ModelState.AddModelError(nameof(CommunityViewModel.BannerImage), text.CoverInvalid);
        }
    }

    // Saves the cover image and points the community at it. Keeps the old image if saving fails.
    // Returns the replaced image's key, to delete once the community is saved (null if none).
    private async Task<string?> TrySaveBannerAsync(Community community, IFormFile? file)
    {
        if (file == null || file.Length == 0 || !_bannerExtensions.TryGetValue(file.ContentType, out var extension))
        {
            return null;
        }

        var key = $"{BANNER_FOLDER}/{Guid.NewGuid():N}{extension}";
        var saved = false;

        var container = BannerContainer();
        if (container != null)
        {
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                // The container may already exist and the key may not allow creating one.
                _logger.LogDebug(ex, "Could not create the blob container.");
            }
            try
            {
                await using var upload = file.OpenReadStream();
                await container.GetBlobClient(key).UploadAsync(upload, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType },
                });
                saved = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not upload cover image to blob storage; saving it locally.");
            }
        }

        if (!saved)
        {
            try
            {
                var path = LocalBannerPath(key);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await using var input = file.OpenReadStream();
                await using var output = System.IO.File.Create(path);
                await input.CopyToAsync(output);
                saved = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not save cover image for community {Id}.", community.Id);
            }
        }

        if (!saved)
        {
            return null;
        }

        var oldKey = community.BannerPhotoStorageKey;
        community.BannerPhotoStorageKey = key;
        community.BannerPhotoOriginalFileName = Path.GetFileName(file.FileName);
        community.BannerPhotoContentType = file.ContentType;
        community.BannerPhotoFileSizeBytes = file.Length;
        community.BannerPhotoUpdatedAt = DateTime.UtcNow;
        return oldKey;
    }

    // Removes a cover image from blob storage and App_Data. Missing files are fine.
    // Used for removing an old image.
    private async Task DeleteBannerAsync(string? key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }
        try
        {
            var container = BannerContainer();
            if (container != null)
            {
                await container.GetBlobClient(key).DeleteIfExistsAsync();
            }
            var path = LocalBannerPath(key);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete old cover image {Key}.", key);
        }
    }

    // Blob container for cover images, or null when blob storage
    // isn't set up (Program.cs).
    private BlobContainerClient? BannerContainer()
    {
        var blobService = HttpContext.RequestServices.GetService<BlobServiceClient>();
        if (blobService == null)
        {
            return null;
        }
        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var containerName = configuration["AzureStorage:ContainerName"];
        return blobService.GetBlobContainerClient(string.IsNullOrWhiteSpace(containerName) ? "media" : containerName);
    }

    // Local copy: App_Data/community-banners/<file>. Only the file name of the key is used.
    private string LocalBannerPath(string key) =>
        Path.Combine(_env.ContentRootPath, "App_Data", BANNER_FOLDER, Path.GetFileName(key));

    // Checks the first bytes so a renamed non-image file can't be saved as a cover.
    private static bool HasImageSignature(IFormFile file)
    {
        var header = new byte[12];
        using var stream = file.OpenReadStream();
        var read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

        bool Matches(int offset, params byte[] signature) =>
            read >= offset + signature.Length
            && header.Skip(offset).Take(signature.Length).SequenceEqual(signature);

        return Matches(0, 0xFF, 0xD8, 0xFF)                                          // JPEG
            || Matches(0, 0x89, 0x50, 0x4E, 0x47)                                    // PNG
            || Matches(0, 0x47, 0x49, 0x46, 0x38)                                    // GIF
            || (Matches(0, 0x52, 0x49, 0x46, 0x46) && Matches(8, 0x57, 0x45, 0x42, 0x50)); // WEBP ("RIFF"/"WEBP")
    }

    private bool CommunityExists(int? id)
    {
        return _context.Communities.Any(e => e.Id == id);
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 20, 2026
    /// Description: Checks if specific member exists in specific community
    ///              Async, must use "await".
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="community">Community</param>
    /// <returns>Bool</returns>
    private async Task<bool> MemberExistsAsync(string? id, Community community)
    {
        var members = await _context.CommunityMembers.Where(m => m.CommunityId == community.Id).ToListAsync();
        foreach (var member in members)
        {
            if (member.UserId == id)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 22, 2026
    /// Description: Checks if specific member exists in specific community.
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="community">Community</param>
    /// <returns>Bool</returns>
    private bool MemberExists(string? id, Community community)
    {
        var members = _context.CommunityMembers.Where(m => m.CommunityId == community.Id).ToList();
        foreach (var member in members)
        {
            if (member.UserId == id)
            {
                return true;
            }
        }
        return false;

    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 14, 2026
    /// Description: Checks if user has access rights to the call.
    /// </summary>
    /// <param name="ownerOnly">bool if access is only for the owner of the Community, otherwise moderators also have access</param>
    /// <return>A bool, true if access, false otherwise</return>
    private bool CommunityAccessChecker(bool ownerOnly, Community community)
    {
        string? currentUserId = _userManager.GetUserId(User);
        // Check if owner
        if (currentUserId == community.CreatorId)
        {
            return true;
        }
        // if owner flag is off, check if moderator
        if (!ownerOnly)
        {
            foreach (CommunityMember member in community.Members)
            {
                if (member.UserId == currentUserId && member.IsModerator)
                {
                    return true;
                }
            }
        }
        return false;
    }

}
