namespace CRM.Application.AuditLogs;

public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string? UserId,
    string? UserName,
    string? Changes,
    string? IpAddress,
    string? UserAgent,
    DateTime Timestamp);

