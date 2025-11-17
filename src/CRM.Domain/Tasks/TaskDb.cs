using CRM.Domain.Abstractions;

namespace CRM.Domain.Tasks;

public class TaskDb : Entity<Guid>, IAuditableEntity
{
    private TaskDb()
    {
    }

    public TaskDb(
        Guid id,
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        DateTime? dueDate,
        Guid? assignedToUserId,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        CreatedAt = DateTime.UtcNow;
    }

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? RelatedEntityType { get; private set; } // e.g., "Customer", "Shipment", etc.
    public Guid? RelatedEntityId { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Update(
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        DateTime? dueDate,
        Guid? assignedToUserId,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(TaskStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid? userId)
    {
        AssignedToUserId = userId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = TaskStatus.Completed;
        LastModifiedAt = DateTime.UtcNow;
    }
}

