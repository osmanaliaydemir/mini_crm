using CRM.Domain.Abstractions;

namespace CRM.Domain.Notifications;

public class EmailAutomationRuleRecipient : Entity<Guid>
{
    private EmailAutomationRuleRecipient()
    {
    }

    public EmailAutomationRuleRecipient(
        Guid id,
        Guid ruleId,
        EmailRecipientType recipientType,
        Guid? userId,
        string? emailAddress,
        string? roleName = null)
    {
        Id = id;
        RuleId = ruleId;
        RecipientType = recipientType;
        UserId = userId;
        EmailAddress = emailAddress;
        RoleName = roleName;
    }

    public Guid RuleId { get; private set; }
    public EmailRecipientType RecipientType { get; private set; }
    public Guid? UserId { get; private set; }
    public string? EmailAddress { get; private set; }
    public string? RoleName { get; private set; }

    public void Update(EmailRecipientType recipientType, Guid? userId, string? emailAddress, string? roleName)
    {
        RecipientType = recipientType;
        UserId = userId;
        EmailAddress = emailAddress;
        RoleName = roleName;
    }
}

