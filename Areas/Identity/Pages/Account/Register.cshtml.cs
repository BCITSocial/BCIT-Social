// author: Andre Di Lascio
// Creates Identity accounts and starts the configured email confirmation flow.
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using BCIT_Social.Data;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string ReturnUrl { get; private set; } = "/";
    public string PasswordHelp
    {
        get
        {
            var options = userManager.Options.Password;
            var requirements = new List<string> { $"at least {options.RequiredLength} characters" };
            if (options.RequireUppercase) requirements.Add("an uppercase letter");
            if (options.RequireLowercase) requirements.Add("a lowercase letter");
            if (options.RequireDigit) requirements.Add("a number");
            if (options.RequireNonAlphanumeric) requirements.Add("a symbol");
            return "Use " + string.Join(", ", requirements) + ".";
        }
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [Required, DataType(DataType.Password), Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : "/";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        OnGet(returnUrl);
        if (!ModelState.IsValid) return Page();

        var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        var profile = new Profile
        {
            UserId = user.Id,
            DisplayName = user.UserName,
            Bio = "",
            Program = "",
            Campus = "",
        };
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null,
            values: new { area = "Identity", userId = user.Id, code, returnUrl = ReturnUrl },
            protocol: Request.Scheme)!;
        await emailSender.SendEmailAsync(Input.Email, "Confirm your BCIT Social email",
            $"Confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        if (userManager.Options.SignIn.RequireConfirmedAccount || userManager.Options.SignIn.RequireConfirmedEmail)
            return RedirectToPage("./RegisterConfirmation", new { email = Input.Email, returnUrl = ReturnUrl });

        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(ReturnUrl);
    }
}
