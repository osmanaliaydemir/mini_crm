using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    [BindProperty]
    public ForgotPasswordInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Şifre sıfırlama akışı eklenecek.
        await Task.CompletedTask;

        TempData["Message"] = "Şifre sıfırlama akışı henüz devrede değil.";
        return RedirectToPage("/Auth/Login");
    }

    public sealed class ForgotPasswordInput
    {
        [Display(Name = "E-posta adresi")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

