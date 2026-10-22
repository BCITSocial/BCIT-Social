
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Models;
using BCIT_Social.Data;
using Microsoft.AspNetCore.Authorization;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Identity;

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

        DetailsCommunityViewModel viewModel = await DetailsViewModelFromCommunity(community);
        return View(viewModel);
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
        };
        var communityMember = new CommunityMember
        {
            UserId = creatorId,
            CommunityId = community.Id,
            IsModerator = true,
        };
        community.Members.Add(communityMember);

        if (ModelState.IsValid)
        {
            _context.Add(community);
            _context.Add(communityMember);
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
        return View(EditViewModelFromCommunity(community));
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

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 19, 2026
    /// Description: GET call to join a community
    /// Redirects to the Join page, checks if the user has not joined this community, otherwise redirects
    /// </summary>
    /// <param name="id">community id</param>
    // GET: COMMUNITYS/Join/5
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

        if (community == null)
        {
            return NotFound();
        }
        if (await MemberExists(currentUserId, community))
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
    // POST: COMMUNITYS/Join/5
    [HttpPost, ActionName("Join")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> JoinConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (await MemberExists(currentUserId, community))
        {
            return RedirectToAction(nameof(Index));
        }

        var communityMember = new CommunityMember
        {
            UserId = currentUserId,
            CommunityId = community.Id,
            IsModerator = false,
        };
        Console.WriteLine(communityMember.User);
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
    // GET: COMMUNITYS/Leave/5
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
            .FirstOrDefaultAsync(m => m.Id == id);

        if (community == null)
        {
            return NotFound();
        }

        if (community.CreatorId == currentUserId)
        {
            return Forbid();
            // TODO: might have to either not show "Leave" on frontend when creator or give a popup when trying to leave.
        }

        if (await MemberExists(currentUserId, community))
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
    // POST: COMMUNITYS/Leave/5
    [HttpPost, ActionName("Leave")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> LeaveConfirmed(int? id)
    {
        var community = await _context.Communities.FindAsync(id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null)
        {
            return NotFound();
        }
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (await MemberExists(currentUserId, community) == false)
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
    /// Last Revised: Oct. 14, 2026
    /// Description: Creates a view model for Community Editing
    /// </summary>
    /// <param name="community">a Community</param>
    /// <returns>a view model</returns>
    private CommunityViewModel EditViewModelFromCommunity(Community community)
    {
        var viewModel = new CommunityViewModel
        {
            Name = community.Name,
            Type = community.Type,
        };
        return viewModel;
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Oct. 19, 2026
    /// Description: Creates a view model for Community Details
    /// </summary>
    /// <param name="community">a Community</param>
    /// <returns>a view model</returns>
    private async Task<DetailsCommunityViewModel> DetailsViewModelFromCommunity(Community community)
    {
        var members = await _context.CommunityMembers
        .Where(m => m.CommunityId == community.Id)
        .Include(u => u.User)
        .ToListAsync();
        var creator = await _userManager.FindByIdAsync(community.CreatorId);
        var creatorName = creator?.UserName;
        var viewModel = new DetailsCommunityViewModel
        {
            Name = community.Name,
            Type = community.Type,
            Creator = creatorName ?? "Not Found",
            Members = members
        };
        return viewModel;
    }

    // GET: COMMUNITYS/Delete/5
    [Authorize]
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
    [Authorize]
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
    /// Last Revised: Oct. 20, 2026
    /// Description: Checks if specific member exists in specific community
    ///              Async, must use "await".
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="community">Community</param>
    /// <returns>Bool</returns>
    private async Task<bool> MemberExists(string? id, Community community)
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
