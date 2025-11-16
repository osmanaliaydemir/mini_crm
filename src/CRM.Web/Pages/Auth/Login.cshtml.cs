using System.ComponentModel.DataAnnotations;
using CRM.Application.AuditLogs;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
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

        // Audit log için bilgileri al (kullanıcı bulunamasa bile)
        var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        if (user is null)
        {
            // Kullanıcı bulunamadı - başarısız giriş denemesi logla (background task olarak)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                    
                    await auditLogService.CreateLogAsync(new CreateAuditLogRequest(
                        "ApplicationUser",
                        Guid.Empty, // Kullanıcı bulunamadı
                        "FailedLogin",
                        null,
                        userName, // Denenen kullanıcı adı/email
                        "User not found",
                        ipAddress,
                        userAgent), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log failed login attempt (user not found)");
                }
            });

            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
            return Page();
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            // Başarısız giriş denemesini logla (background task olarak)
            var failedUserId = user.Id;
            var failedUserName = user.UserName;
            var lockedOut = signInResult.IsLockedOut;
            var notAllowed = signInResult.IsNotAllowed;
            
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                    
                    await auditLogService.CreateLogAsync(new CreateAuditLogRequest(
                        "ApplicationUser",
                        failedUserId,
                        "FailedLogin",
                        failedUserId.ToString(),
                        failedUserName,
                        $"Failed login attempt. LockedOut: {lockedOut}, NotAllowed: {notAllowed}",
                        ipAddress,
                        userAgent), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log failed login attempt");
                }
            });

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

        // Başarılı girişi logla (background task olarak)
        var loginUserId = user.Id;
        var loginUserName = user.UserName;
        var rememberMe = Input.RememberMe;
        
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                
                await auditLogService.CreateLogAsync(new CreateAuditLogRequest(
                    "ApplicationUser",
                    loginUserId,
                    "Login",
                    loginUserId.ToString(),
                    loginUserName,
                    $"Successful login. RememberMe: {rememberMe}",
                    ipAddress,
                    userAgent), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log successful login");
            }
        });

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

