
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

    // GET: COMMUNITY
    public async Task<IActionResult> Index()
    {
        try
        {
            var communities = IndexViewModelFromCommunity(await _context.Communities.ToListAsync());
            return View(communities);
        }
        catch
        {
            // Fallback on macOS since I can't connect to the DB!
            return View(new List<Community>());
        }
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
        if (community == null)
        {
            return NotFound();
        }

        CommunityMembersViewModel viewModel = await MembersViewModelFromCommunity(community);
        return View(viewModel);
    }


    // GET: COMMUNITY/Create
    public IActionResult Create()
    {
        return View(new CommunityViewModel());
    }

    // POST: COMMUNITY/Create
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

    // GET: COMMUNITY/Edit/5
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

    // POST: COMMUNITY/Edit/5
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
            return RedirectToAction(nameof(Details), new
            {
                community.Id
            });
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

        if (community == null)
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
        if (community == null)
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
        if (community == null)
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
        Console.WriteLine("\n\nAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\n\n");
        var communityMember = await _context.CommunityMembers.Include(m => m.User).Where(m => m.UserId == userid && m.CommunityId == id).FirstAsync();

        string? currentUserId = _userManager.GetUserId(User);
        var currentCommunityUser = await _context.CommunityMembers.FindAsync(currentUserId, id);
        var community = await _context.Communities.FirstOrDefaultAsync(m => m.Id == id);

        if (id == null || userid == null || communityMember == null || community == null)
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
            Console.WriteLine("\n\nBBBBBBBBBBBBBBB\n\n");
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
        Console.WriteLine("\n\nCCCCCCCC\n\n");
        var community = await _context.Communities.FindAsync(id);
        var communityMember = await _context.CommunityMembers.FindAsync(userid, id);
        string? currentUserId = _userManager.GetUserId(User);
        if (community == null || communityMember == null)
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
            Console.WriteLine("\n\nEEEEE\n\n");
            return RedirectToAction(nameof(Index));
        }
        if (ModelState.IsValid)
        {
            Console.WriteLine("\n\nDDDDDDDD\n\n");
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
        var communityMember = await _context.CommunityMembers.Include(m => m.User).Where(m => m.UserId == userid && m.CommunityId == id).FirstAsync();
        string? currentUserId = _userManager.GetUserId(User);
        var currentCommunityUser = await _context.CommunityMembers.FindAsync(currentUserId, id);
        var community = await _context.Communities.FirstOrDefaultAsync(m => m.Id == id);

        if (id == null || userid == null || communityMember == null || community == null)
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
        if (community == null || communityMember == null)
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
            Console.WriteLine(222222222);
            Console.WriteLine(viewModel.IsModerator);
            communityMember.IsModerator = viewModel.IsModerator;
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

    // POST: COMMUNITY/Delete/5
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

    // ---------------------
    // CREATING VIEW MODELS
    // ---------------------

    private List<CommunityViewModel> IndexViewModelFromCommunity(List<Community> communities)
    {
        var currentUserId = _userManager.GetUserId(User);
        List<CommunityViewModel> communityViewModels = [];
        foreach (var community in communities)
        {
            bool isJoined = MemberExists(currentUserId, community);
            var viewModel = new CommunityViewModel
            {
                Id = community.Id,
                Name = community.Name,
                Type = community.Type,
                Description = community.Description,
                IsJoined = isJoined,
                MembersCount = community.Members.Count,
            };
            communityViewModels.Add(viewModel);
        }
        return communityViewModels;
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
            // author: Jaspreet Bath
            Description = community.Description
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


        var viewModel = new CommunityDetailsViewModel
        {
            Id = community.Id,
            Name = community.Name,
            Type = community.Type,
            Description = community.Description,
            Creator = creatorName ?? "Not Found",
            Members = members,
            IsMod = isMod,
            IsMember = isMember,
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
        Console.WriteLine("\n\n!!!!!!!!!!!!!!!!!!!!!!!!");
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

    // -------
    // HELPERS
    // -------


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
