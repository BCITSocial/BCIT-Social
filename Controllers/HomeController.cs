using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BCIT_Social.Models;
using Microsoft.EntityFrameworkCore;
using BCIT_Social.Data;

namespace BCIT_Social.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

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
}
