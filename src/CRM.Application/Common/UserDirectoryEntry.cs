namespace CRM.Application.Common;

public record UserDirectoryEntry(
    Guid Id,
    string DisplayName,
    string? Email,
    IReadOnlyCollection<string> Roles);

