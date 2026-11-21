using Azure;
using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BCIT_Social.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Returns Main Admin Page
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var communities = await _context.Communities
                                .Include(m => m.Members)
                                .OrderByDescending(m => m.DateOfDel)
                                .ToListAsync();
        var users = await _userManager.Users
                                .OrderByDescending(m => m.DateOfDel)
                                .ToListAsync();
        AdminViewModel viewModel = new AdminViewModel
        {
            Communities = communities,
            Users = users
        };
        return View(viewModel);
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Returns Community Edit Page for Admin
    /// </summary>
    public async Task<IActionResult> EditCommunity(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var community = await _context.Communities
                                    .Where(c => c.Id == id)
                                    .Include(c => c.Creator)
                                    .Include(c => c.Members)
                                    .ThenInclude(m => m.User)
                                    .FirstOrDefaultAsync();
        if (community == null)
        {
            return NotFound();
        }
        return View(community);
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Restores a community
    /// Sets DateOfDel to null
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreCommunity(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var community = await _context.Communities
                            .Where(c => c.Id == id)
                            .FirstOrDefaultAsync();
        if (community == null)
        {
            return NotFound();
        }
        if (community.DateOfDel == null)
        {
            return RedirectToAction(nameof(EditCommunity), new { id });
        }
        if (ModelState.IsValid)
        {
            community.DateOfDel = null;
        }

        _context.Update(community);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditCommunity), new { id });
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Deletes a Community
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCommunity(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var community = await _context.Communities
                            .Where(c => c.Id == id)
                            .FirstOrDefaultAsync();
        if (community == null)
        {
            return NotFound();
        }
        if (community.DateOfDel != null)
        {
            return RedirectToAction(nameof(EditCommunity), new { id });
        }
        if (ModelState.IsValid)
        {
            community.DateOfDel = DateTime.Now.AddDays(7);
        }

        _context.Update(community);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditCommunity), new { id });
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Deletes a Community Permenantly
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermaDeleteCommunity(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var community = await _context.Communities
                            .Where(c => c.Id == id)
                            .FirstOrDefaultAsync();
        if (community == null)
        {
            return NotFound();
        }
        if (community.DateOfDel == null)
        {
            return RedirectToAction(nameof(EditCommunity), new { id });
        }

        _context.Communities.Remove(community);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Returns User Edit Page for Admin
    /// </summary>
    public async Task<IActionResult> EditUser(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var communities = await _context.Communities
                                    .Where(c => c.CreatorId == id)
                                    .Where(c => c.DateOfDel == null)
                                    .ToListAsync();
        var viewModel = new AdminUserViewModel
        {
            AppUser = user,
            CommunitiesOwned = communities
        };
        return View(viewModel);
    }
    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Restores a community
    /// Sets DateOfDel to null
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreUser(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        if (user.DateOfDel == null)
        {
            return RedirectToAction(nameof(EditUser), new { id });
        }
        if (ModelState.IsValid)
        {
            user.DateOfDel = null;
        }
        _context.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditUser), new { id });
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Deletes a User
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var user = await _userManager.FindByIdAsync(id);
        var communities = await _context.Communities
                .Where(c => c.CreatorId == id)
                .Where(c => c.DateOfDel == null)
                .ToListAsync();
        if (user == null)
        {
            return NotFound();
        }
        if (user.DateOfDel != null)
        {
            return RedirectToAction(nameof(EditUser), new { id });
        }
        if (ModelState.IsValid)
        {
            user.DateOfDel = DateTime.Now.AddDays(7);
            foreach (var comm in communities)
            {
                comm.DateOfDel = DateTime.Now.AddDays(7); ;
                _context.Update(comm);
            }
        }

        _context.Update(user);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditUser), new { id });
    }

    /// <summary>
    /// Author: Tin Trinh
    /// Last Revised: Nov. 21, 2026
    /// Deletes a User Permenantly
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermaDeleteUser(string? id)
    {
        // var currentUser = await _userManager.GetUserIdAsync(User);
        if (id == null)
        {
            return NotFound();
        }
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        if (user.DateOfDel == null)
        {
            return RedirectToAction(nameof(EditUser), new { id });
        }

        await _userManager.DeleteAsync(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}