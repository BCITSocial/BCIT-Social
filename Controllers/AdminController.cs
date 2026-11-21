using BCIT_Social.Data;
using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> Index()
    {
        var communities = await _context.Communities
                                .Include(m => m.Members)
                                .ToListAsync();
        var users = await _userManager.Users.ToListAsync();
        AdminViewModel viewModel = new AdminViewModel
        {
            Communities = communities,
            Users = users
        };
        return View(viewModel);
    }
}