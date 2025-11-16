namespace CRM.Application.Users;

public record UserDto(
    Guid Id,
    string UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    string? Locale,
    bool EmailConfirmed,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    int AccessFailedCount,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Roles);

