using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Profile;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public ProfileInput Profile { get; set; } = new();

    [BindProperty]
    public ChangePasswordInput ChangePassword { get; set; } = new();

    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public IList<string> Roles { get; set; } = Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        await LoadUserDataAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadUserDataAsync(user);
            return Page();
        }

        try
        {
            user.FirstName = Profile.FirstName;
            user.LastName = Profile.LastName;
            user.Locale = Profile.Locale;
            user.PhoneNumber = Profile.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User profile updated. UserId: {UserId}", user.Id);
                TempData["StatusMessage"] = "Profile updated successfully.";
                TempData["StatusMessageType"] = "success";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile. UserId: {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating your profile. Please try again.");
        }

        await LoadUserDataAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadUserDataAsync(user);
            return Page();
        }

        try
        {
            var result = await _userManager.ChangePasswordAsync(
                user,
                ChangePassword.CurrentPassword,
                ChangePassword.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("User password changed. UserId: {UserId}", user.Id);
                TempData["StatusMessage"] = "Password changed successfully.";
                TempData["StatusMessageType"] = "success";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    ModelState.AddModelError(nameof(ChangePassword.CurrentPassword), "Current password is incorrect.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password. UserId: {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while changing your password. Please try again.");
        }

        await LoadUserDataAsync(user);
        return Page();
    }

    private async Task LoadUserDataAsync(ApplicationUser user)
    {
        UserName = user.UserName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        Roles = await _userManager.GetRolesAsync(user);

        Profile.FirstName = user.FirstName ?? string.Empty;
        Profile.LastName = user.LastName ?? string.Empty;
        Profile.Locale = user.Locale ?? "tr-TR";
        Profile.PhoneNumber = user.PhoneNumber ?? string.Empty;
    }

    public sealed class ProfileInput
    {
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Language")]
        public string Locale { get; set; } = "tr-TR";

        [Display(Name = "Phone Number")]
        [Phone]
        [MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public sealed class ChangePasswordInput
    {
        [Display(Name = "Current Password")]
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Display(Name = "New Password")]
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Confirm New Password")]
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

