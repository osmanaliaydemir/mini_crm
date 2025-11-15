using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CRM.Web.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(
        UserManager<ApplicationUser> userManager,
        ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    public string? Email { get; set; }
    public string? Token { get; set; }
    public bool IsValidToken { get; set; } = true;

    public IActionResult OnGet(string? token, string? email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Geçersiz şifre sıfırlama bağlantısı.";
            return RedirectToPage("/Auth/ForgotPassword");
        }

        Token = token;
        Email = email;
        Input.Email = email;
        Input.Token = token;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            IsValidToken = true;
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.Token) || string.IsNullOrWhiteSpace(Input.Email))
        {
            ModelState.AddModelError(string.Empty, "Geçersiz şifre sıfırlama bağlantısı.");
            IsValidToken = false;
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            // Güvenlik: Kullanıcı bulunamasa bile genel bir hata mesajı göster
            ModelState.AddModelError(string.Empty, "Şifre sıfırlama işlemi başarısız oldu. Lütfen yeni bir şifre sıfırlama talebinde bulunun.");
            IsValidToken = false;
            return Page();
        }

        try
        {
            // Token'ı decode et
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Token));

            // Şifreyi sıfırla
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Şifre başarıyla sıfırlandı. Kullanıcı: {Email}", Input.Email);
                TempData["Message"] = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToPage("/Auth/Login");
            }

            // Hataları ModelState'e ekle
            foreach (var error in result.Errors)
            {
                if (error.Code == "InvalidToken")
                {
                    ModelState.AddModelError(string.Empty, "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş. Lütfen yeni bir şifre sıfırlama talebinde bulunun.");
                    IsValidToken = false;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre sıfırlama işlemi sırasında hata oluştu. Kullanıcı: {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
            IsValidToken = false;
        }

        return Page();
    }

    public sealed class ResetPasswordInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Display(Name = "Yeni şifre")]
        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Yeni şifre (tekrar)")]
        [Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

