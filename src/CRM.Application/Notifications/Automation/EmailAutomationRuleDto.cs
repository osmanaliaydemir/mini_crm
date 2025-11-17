using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public record EmailAutomationRuleDto(
    Guid Id,
    string Name,
    EmailResourceType ResourceType,
    EmailTriggerType TriggerType,
    EmailExecutionType ExecutionType,
    string TemplateKey,
    string? CronExpression,
    string? TimeZoneId,
    Guid? RelatedEntityId,
    bool IsActive,
    string? Metadata,
    IReadOnlyCollection<EmailAutomationRecipientDto> Recipients,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? LastModifiedAt,
    string? LastModifiedBy,
    byte[] RowVersion);

