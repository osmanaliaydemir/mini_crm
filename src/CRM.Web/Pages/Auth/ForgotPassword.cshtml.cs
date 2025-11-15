using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using CRM.Application.Common;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CRM.Web.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
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

            // E-posta içeriği oluştur
            var emailSubject = "Şifre Sıfırlama Talebi";
            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Şifre Sıfırlama Talebi</h2>
                        <p>Merhaba,</p>
                        <p>Hesabınız için şifre sıfırlama talebinde bulunuldu. Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
                        <p style='margin: 30px 0;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}' 
                               style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Şifremi Sıfırla
                            </a>
                        </p>
                        <p>Veya aşağıdaki bağlantıyı tarayıcınıza kopyalayıp yapıştırabilirsiniz:</p>
                        <p style='word-break: break-all; color: #666; font-size: 12px;'>{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}</p>
                        <p style='margin-top: 30px; color: #666; font-size: 12px;'>
                            <strong>Not:</strong> Bu bağlantı 24 saat geçerlidir. Eğer bu talebi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.
                        </p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
                        <p style='color: #999; font-size: 11px;'>Bu otomatik bir e-postadır, lütfen yanıtlamayın.</p>
                    </div>
                </body>
                </html>";

            // E-postayı gönder
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

