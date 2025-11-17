using CRM.Domain.Tasks;

namespace CRM.Application.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    Domain.Tasks.TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? LastModifiedAt,
    byte[] RowVersion);

public record CreateTaskRequest(
    string Title,
    string? Description,
    Domain.Tasks.TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    string? RelatedEntityType,
    Guid? RelatedEntityId);

public record UpdateTaskRequest(
    Guid Id,
    string Title,
    string? Description,
    Domain.Tasks.TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    byte[] RowVersion);

