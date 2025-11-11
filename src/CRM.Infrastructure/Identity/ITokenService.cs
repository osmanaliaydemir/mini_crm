using CRM.Application.Authentication;

namespace CRM.Infrastructure.Identity;

public interface ITokenService
{
    Task<AuthTokens> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<AuthTokens> RefreshTokensAsync(ApplicationUser user, string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}

