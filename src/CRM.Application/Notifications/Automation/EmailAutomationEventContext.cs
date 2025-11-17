using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public class EmailAutomationEventContext
{
    public EmailResourceType ResourceType { get; init; }
    public EmailTriggerType TriggerType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string TemplateKey { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public IDictionary<string, string> Placeholders { get; init; } = new Dictionary<string, string>();
    public IReadOnlyCollection<Guid> AdditionalUserIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<string> AdditionalEmails { get; init; } = Array.Empty<string>();
    public bool ForceSendWhenNoRule { get; init; }
}

