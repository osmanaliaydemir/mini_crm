namespace CRM.Application.Authentication;

public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

