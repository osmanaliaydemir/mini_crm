using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CRM.Application.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CRM.Infrastructure.Identity;

public sealed class JwtTokenService : ITokenService
{
    private const string RefreshTokenProvider = "CRM";
    private const string RefreshTokenName = "RefreshToken";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthTokens> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await BuildClaimsAsync(user, roles);

        var accessToken = CreateJwtToken(claims);
        var refreshToken = GenerateSecureToken();

        await StoreRefreshTokenAsync(user, refreshToken, cancellationToken);

        return new AuthTokens(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays));
    }

    public async Task<AuthTokens> RefreshTokensAsync(ApplicationUser user, string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _userManager.GetAuthenticationTokenAsync(user, RefreshTokenProvider, RefreshTokenName);

        if (storedToken is null || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedToken),
                Encoding.UTF8.GetBytes(refreshToken)))
        {
            throw new SecurityTokenException("Geçersiz yenileme jetonu.");
        }

        await _userManager.RemoveAuthenticationTokenAsync(user, RefreshTokenProvider, RefreshTokenName);

        return await GenerateTokensAsync(user, cancellationToken);
    }

    public Task RevokeRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, RefreshTokenProvider, RefreshTokenName);
    }

    private async Task StoreRefreshTokenAsync(ApplicationUser user, string refreshToken, CancellationToken cancellationToken)
    {
        await _userManager.RemoveAuthenticationTokenAsync(user, RefreshTokenProvider, RefreshTokenName);

        var setResult = await _userManager.SetAuthenticationTokenAsync(
            user,
            RefreshTokenProvider,
            RefreshTokenName,
            refreshToken);

        if (!setResult.Succeeded)
        {
            var errors = string.Join(",", setResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Yenileme jetonu kaydedilemedi. {errors}");
        }
    }

    private async Task<IList<Claim>> BuildClaimsAsync(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;
    }

    private (string Token, DateTime ExpiresAt) CreateJwtToken(IEnumerable<Claim> claims)
    {
        var expiration = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}

