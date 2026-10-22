
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Authorization;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CommunityController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommunityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: COMMUNITYS
    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await _context.Communities.ToListAsync());
        }
        catch
        {
            // Fallback on macOS since I can't connect to the DB!
            return View(new List<Community>());
        }
    }

    // GET: COMMUNITYS/Details/5
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

        return View(community);
    }


    // Author: Jaspreet Bath
    // Allows an auth user to join communities by adding a new CommunityMember record.
    // updates the community status tag to "Going" from communities-text.json,
    // and returns the user to the main index page.
    // POST: Community/Join/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Join(int id)
    {
        // gets community and its members from db
        var community = await _context.Communities
            // eager loading to pull related data from db
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id);

        // redirect id empty/null to 404
        if (community == null)
        {
            return NotFound();
        }

        // confirming user auth if logged in or not
        // if so, return: back to login page
        var currentUserId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Challenge();
        }

        // if DB members is empty, then make empty one
        // crash pweventing
        if (community.Members == null)
        {
            community.Members = new List<CommunityMember>();
        }

        // add a member record if not already joined.
        // this is for user who just made an account and is
        // trying to make a post. id auth is needed bruv.
        if (!community.Members.Any(m => m.UserId == currentUserId))
        {
            community.Members.Add(new CommunityMember
            {
                CommunityId = id,
                UserId = currentUserId,
                IsModerator = false
            });
        }

        // wwwroot/content/communities-text.json
        var jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "communities-text.json");
        if (System.IO.File.Exists(jsonPath))
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(jsonPath));
            var root = jsonDoc.RootElement;

            // looks for key, puts in out var modalElem, then checks for tag names
            var statusOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (root.TryGetProperty("createModal", out var modalElem) &&
                modalElem.TryGetProperty("statusOptions", out var statusList))
            {
                foreach (var opt in statusList.EnumerateArray())
                {
                    var optText = opt.GetString();
                    // validation check before setting
                    if (!string.IsNullOrWhiteSpace(optText) && !optText.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        statusOptions.Add(optText);
                    }
                }
            }

            // looks for tags with "going", sorts if status tag
            var targetStatus = "Going";
            if (root.TryGetProperty("featured", out var featuredElem) &&
                featuredElem.TryGetProperty("statusTag", out var statusTagElem) &&
                !string.IsNullOrWhiteSpace(statusTagElem.GetString()))
            {
                targetStatus = statusTagElem.GetString()!;
            }

            // strips existing tags
            var tags = (community.Type ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(t => !statusOptions.Contains(t))
                .ToList();

            // joing tag to community type (sports, tech, etc)
            tags.Add(targetStatus);
            community.Type = string.Join(", ", tags);
        }

        // async on db changes (happens after making a post)
        await _context.SaveChangesAsync();

        // redirects user back to where he made post, fallback is to Index
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && Uri.IsWellFormedUriString(referer, UriKind.RelativeOrAbsolute))
        {
            return Redirect(referer);
        }

        return RedirectToAction(nameof(Index));
    }



    // GET: COMMUNITYS/Create
    public IActionResult Create()
    {
        return View(new CommunityViewModel());
    }

    // POST: COMMUNITYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(CommunityViewModel viewModel)
    {
        string? creatorId = _userManager.GetUserId(User);

        if (creatorId == null)
        {
            return Challenge();
        }

        var community = new Community
        {
            CreatorId = creatorId,
            Name = viewModel.Name,
            Type = viewModel.Type,
            // Author: Jaspreet Bath
            Description = viewModel.Description
        };
        if (ModelState.IsValid)
        {
            _context.Add(community);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new
            {
                community.Id
            });
        }
        return View(viewModel);
    }

    // GET: COMMUNITYS/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var community = await _context.Communities.FindAsync(id);
        if (community == null)
        {
            return NotFound();
        }
        if (!CommunityAccessChecker(true, community))
        {
            return Forbid();
        }
        return View(ViewModelFromCommunity(community));
    }

    // POST: COMMUNITYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int? id, CommunityViewModel viewModel)
    {
        var community = await _context.Communities.FindAsync(id);

        if (community == null)
        {
            return NotFound();
        }
        if (!CommunityAccessChecker(true, community))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            community.Name = viewModel.Name;
            community.Type = viewModel.Type;
            // Author: Jaspreet Bath
            community.Description = viewModel.Description;
            try
            {
                _context.Update(community);
                await _context.SaveChangesAsync();
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
            return RedirectToAction(nameof(Index));
        }
        return View(community);
    }

    private CommunityViewModel ViewModelFromCommunity(Community community)
    {
        var viewModel = new CommunityViewModel
        {
            Name = community.Name,
            Type = community.Type,
            // author: Jaspreet Bath
            Description = community.Description
        };
        return viewModel;
    }

    // GET: COMMUNITYS/Delete/5
    public async Task<IActionResult> Delete(int? id)
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
        if (!CommunityAccessChecker(true, community))
        {
            return Forbid();
        }

        return View(community);
    }

    // POST: COMMUNITYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        if (community != null)
        {
            CommunityAccessChecker(true, community);
            _context.Communities.Remove(community);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CommunityExists(int? id)
    {
        return _context.Communities.Any(e => e.Id == id);
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
