using CRM.Domain.Abstractions;

namespace CRM.Domain.Audit;

public class AuditLog : Entity<Guid>
{
    private AuditLog()
    {
    }

    public AuditLog(
        string entityType,
        Guid entityId,
        string action,
        string? userId,
        string? userName,
        string? changes,
        string? ipAddress,
        string? userAgent)
    {
        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        UserId = userId;
        UserName = userName;
        Changes = changes;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Timestamp = DateTime.UtcNow;
    }

    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty; // Created, Updated, Deleted
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Changes { get; private set; } // JSON formatında değişiklikler
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime Timestamp { get; private set; }
}

