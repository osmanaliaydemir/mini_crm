using CRM.Application.AuditLogs;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Web.Pages.Auth;

[Authorize]
public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // Kullanıcı bilgilerini logout'tan önce al
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id ?? Guid.Empty;
        var userName = user?.UserName;

        // Audit log için bilgileri al
        var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        await _signInManager.SignOutAsync();

        // Logout'u logla (background task olarak)
        if (user != null)
        {
            var logoutUserId = userId;
            var logoutUserName = userName;
            
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                    
                    await auditLogService.CreateLogAsync(new CreateAuditLogRequest(
                        "ApplicationUser",
                        logoutUserId,
                        "Logout",
                        logoutUserId.ToString(),
                        logoutUserName,
                        "User logged out",
                        ipAddress,
                        userAgent), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log logout");
                }
            });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Auth/Login");
    }
}

