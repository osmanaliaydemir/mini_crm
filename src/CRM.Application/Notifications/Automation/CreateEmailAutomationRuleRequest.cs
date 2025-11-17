using System.ComponentModel.DataAnnotations;
using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public class CreateEmailAutomationRuleRequest
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public EmailResourceType ResourceType { get; set; }

    [Required]
    public EmailTriggerType TriggerType { get; set; }

    [Required]
    public EmailExecutionType ExecutionType { get; set; }

    [Required]
    [MaxLength(128)]
    public string TemplateKey { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? CronExpression { get; set; }

    [MaxLength(128)]
    public string? TimeZoneId { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Metadata { get; set; }

    public List<EmailAutomationRecipientRequest> Recipients { get; set; } = new();
}

