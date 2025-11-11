namespace CRM.Application.Authentication;

public sealed record AuthResponse(
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles,
    AuthTokens Tokens);

