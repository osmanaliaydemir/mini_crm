namespace CRM.Application.AuditLogs;

public record CreateAuditLogRequest(
    string EntityType,
    Guid EntityId,
    string Action,
    string? UserId = null,
    string? UserName = null,
    string? Changes = null,
    string? IpAddress = null,
    string? UserAgent = null);

