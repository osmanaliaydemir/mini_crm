using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using CRM.Application.Common;
using CRM.Infrastructure.Identity;
using CRM.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILogger<ForgotPasswordModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
        IEmailTemplateService emailTemplateService, IStringLocalizer<SharedResource> localizer, ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _emailTemplateService = emailTemplateService;
        _localizer = localizer;
        _logger = logger;
    }

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

        var user = await _userManager.FindByEmailAsync(Input.Email);

        // Güvenlik: Kullanıcı bulunamasa bile başarı mesajı göster
        // Bu, e-posta adreslerinin sistemde olup olmadığını keşfetmeyi engeller
        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            // Kullanıcı bulunamadı veya e-posta onaylanmamış
            // Ancak güvenlik nedeniyle her zaman başarı mesajı gösteriyoruz
            TempData["Message"] = "Eğer bu e-posta adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir.";
            return RedirectToPage("/Auth/Login");
        }

        try
        {
            // Şifre sıfırlama token'ı oluştur
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));

            // Reset password sayfasına yönlendirme URL'si oluştur
            var callbackUrl = Url.Page(
                "/Auth/ResetPassword",
                pageHandler: null,
                values: new { token, email = Input.Email },
                protocol: Request.Scheme);

            var emailSubject = _localizer["Email_Subject_PasswordReset"].Value;
            var placeholders = new Dictionary<string, string>
            {
                ["Title"] = _localizer["Email_PasswordReset_Title"].Value,
                ["Description"] = _localizer["Email_PasswordReset_Description"].Value,
                ["ActionUrl"] = HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty),
                ["ActionText"] = _localizer["Email_PasswordReset_Action"].Value,
                ["FallbackInstruction"] = _localizer["Email_PasswordReset_FallbackInstruction"].Value,
                ["FallbackUrl"] = HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty),
                ["Note"] = _localizer["Email_PasswordReset_Note"].Value
            };

            var emailBody = await _emailTemplateService.RenderTemplateAsync(
                "PasswordReset",
                placeholders,
                cancellationToken);

            await _emailSender.SendEmailAsync(user.Email!, emailSubject, emailBody, cancellationToken);

            _logger.LogInformation("Şifre sıfırlama e-postası gönderildi. Kullanıcı: {Email}", Input.Email);

            TempData["Message"] = "Eğer bu e-posta adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre sıfırlama e-postası gönderilirken hata oluştu. Kullanıcı: {Email}", Input.Email);
            TempData["Message"] = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        }

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

