namespace CRM.Application.Timeline;

public enum ActivityType
{
    Shipment,
    Customer,
    Task,
    Finance,
    Warehouse,
    Supplier,
    CustomerInteraction,
    EmailAutomation,
    Other
}

public enum ActivityAction
{
    Created,
    Updated,
    Deleted,
    StatusChanged,
    Assigned,
    Completed,
    NoteAdded,
    InteractionAdded
}

public sealed record ActivityTimelineItem(
    Guid Id,
    ActivityType Type,
    ActivityAction Action,
    DateTime OccurredAt,
    string? EntityId,
    string? EntityName,
    string? EntityReference,
    string? Description,
    string? UserId,
    string? UserName,
    string? RelatedEntityType,
    string? RelatedEntityId,
    Dictionary<string, object>? Metadata);

public sealed record ActivityTimelineFilter(
    ActivityType? Type = null,
    ActivityAction? Action = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? UserId = null,
    string? EntityId = null,
    string? EntityType = null,
    int PageNumber = 1,
    int PageSize = 50);

public sealed record ActivityTimelineResult(
    IReadOnlyList<ActivityTimelineItem> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

