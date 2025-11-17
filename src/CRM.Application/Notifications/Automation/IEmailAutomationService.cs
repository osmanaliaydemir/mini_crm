using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public interface IEmailAutomationService
{
    Task<Guid> CreateRuleAsync(CreateEmailAutomationRuleRequest request, CancellationToken cancellationToken = default);
    Task UpdateRuleAsync(UpdateEmailAutomationRuleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailAutomationRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailAutomationRuleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task HandleEventAsync(EmailAutomationEventContext context, CancellationToken cancellationToken = default);
    Task ExecuteScheduledRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailAutomationRuleDto>> GetActiveRulesByExecutionTypeAsync(EmailExecutionType executionType, CancellationToken cancellationToken = default);
}

