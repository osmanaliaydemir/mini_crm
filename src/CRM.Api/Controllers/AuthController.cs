using System.Linq;
using CRM.Application.Authentication;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await FindUserAsync(request.UserNameOrEmail);
        if (user is null)
        {
            return Unauthorized("Geçersiz kullanıcı adı veya parola.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized("Hesabınız kilitlenmiştir. Lütfen yöneticiyle iletişime geçin.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized("Hesabınız çok fazla hatalı giriş nedeniyle geçici olarak kilitlendi.");
            }

            return Unauthorized("Geçersiz kullanıcı adı veya parola.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var tokens = await _tokenService.GenerateTokensAsync(user, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);

        var response = new AuthResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles.ToArray(),
            tokens);

        _logger.LogInformation("Kullanıcı giriş yaptı: {UserId}", user.Id);

        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Unauthorized("Kullanıcı bulunamadı.");
        }

        var tokens = await _tokenService.RefreshTokensAsync(user, request.RefreshToken, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);

        var response = new AuthResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles.ToArray(),
            tokens);

        _logger.LogInformation("Yenileme jetonu kullanıldı: {UserId}", user.Id);

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NoContent();
        }

        await _tokenService.RevokeRefreshTokenAsync(user, cancellationToken);

        _logger.LogInformation("Kullanıcı çıkış yaptı: {UserId}", user.Id);

        return NoContent();
    }

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
    {
        ApplicationUser? user;
        if (userNameOrEmail.Contains('@', StringComparison.Ordinal))
        {
            user = await _userManager.FindByEmailAsync(userNameOrEmail);
        }
        else
        {
            user = await _userManager.FindByNameAsync(userNameOrEmail);
        }

        return user;
    }
}

