using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return LocalRedirect(string.IsNullOrWhiteSpace(ReturnUrl) ? "/" : ReturnUrl!);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userName = Input.UserNameOrEmail.Trim();
        ApplicationUser? user = await _userManager.FindByNameAsync(userName);

        if (user is null && userName.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(userName);
        }

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
            return Page();
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
            }

            return Page();
        }

        return LocalRedirect(string.IsNullOrWhiteSpace(ReturnUrl) ? "/" : ReturnUrl!);
    }

    public sealed class LoginInput
    {
        [Display(Name = "Kullanıcı adı veya e-posta")]
        [Required]
        [MaxLength(256)]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Display(Name = "Parola")]
        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}

