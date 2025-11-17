using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public record EmailAutomationRecipientDto(
    Guid Id,
    EmailRecipientType RecipientType,
    Guid? UserId,
    string? EmailAddress,
    string? RoleName);

