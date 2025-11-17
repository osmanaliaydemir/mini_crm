using CRM.Domain.Abstractions;

namespace CRM.Domain.Notifications;

public class EmailAutomationRule : Entity<Guid>, IAuditableEntity
{
    private EmailAutomationRule()
    {
    }

    public EmailAutomationRule(
        Guid id,
        string name,
        EmailResourceType resourceType,
        EmailTriggerType triggerType,
        EmailExecutionType executionType,
        string templateKey,
        string? cronExpression,
        string? timeZoneId,
        Guid? relatedEntityId,
        bool isActive,
        string? metadata = null)
    {
        Id = id;
        Name = name;
        ResourceType = resourceType;
        TriggerType = triggerType;
        ExecutionType = executionType;
        TemplateKey = templateKey;
        CronExpression = cronExpression;
        TimeZoneId = timeZoneId;
        RelatedEntityId = relatedEntityId;
        Metadata = metadata;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public EmailResourceType ResourceType { get; private set; }
    public EmailTriggerType TriggerType { get; private set; }
    public EmailExecutionType ExecutionType { get; private set; }
    public string TemplateKey { get; private set; } = string.Empty;
    public string? CronExpression { get; private set; }
    public string? TimeZoneId { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? Metadata { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    private readonly List<EmailAutomationRuleRecipient> _recipients = new();
    public ICollection<EmailAutomationRuleRecipient> Recipients => _recipients;

    public void Update(
        string name,
        EmailResourceType resourceType,
        EmailTriggerType triggerType,
        EmailExecutionType executionType,
        string templateKey,
        string? cronExpression,
        string? timeZoneId,
        Guid? relatedEntityId,
        bool isActive,
        string? metadata)
    {
        Name = name;
        ResourceType = resourceType;
        TriggerType = triggerType;
        ExecutionType = executionType;
        TemplateKey = templateKey;
        CronExpression = cronExpression;
        TimeZoneId = timeZoneId;
        RelatedEntityId = relatedEntityId;
        Metadata = metadata;
        IsActive = isActive;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetRecipients(IEnumerable<EmailAutomationRuleRecipient> recipients)
    {
        _recipients.Clear();

        if (recipients == null)
        {
            return;
        }

        _recipients.AddRange(recipients);
    }
}

