using System.ComponentModel.DataAnnotations;
using CRM.Domain.Notifications;

namespace CRM.Application.Notifications.Automation;

public class EmailAutomationRecipientRequest
{
    [Required]
    public EmailRecipientType RecipientType { get; set; }

    public Guid? UserId { get; set; }

    [EmailAddress]
    public string? EmailAddress { get; set; }

    [MaxLength(128)]
    public string? RoleName { get; set; }
}

