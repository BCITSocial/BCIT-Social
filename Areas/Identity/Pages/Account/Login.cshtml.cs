// author: Andre Di Lascio
// Validates login details and signs users in through ASP.NET Core Identity.
using System.ComponentModel.DataAnnotations;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BCIT_Social.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string ReturnUrl { get; private set; } = "/";

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : "/";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        OnGet(returnUrl);
        if (!ModelState.IsValid) return Page();

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return LocalRedirect(ReturnUrl);
        if (result.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl, Input.RememberMe });
        if (result.IsLockedOut) return RedirectToPage("./Lockout");

        ModelState.AddModelError(string.Empty,
            "Unable to log in. Check your email and password, and confirm your email if you just signed up.");
        return Page();
    }
}
