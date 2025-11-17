using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public interface IEmailAutomationScheduler
{
    Task ScheduleAsync(EmailAutomationRule rule, CancellationToken cancellationToken = default);
    Task UnscheduleAsync(Guid ruleId, CancellationToken cancellationToken = default);
}

