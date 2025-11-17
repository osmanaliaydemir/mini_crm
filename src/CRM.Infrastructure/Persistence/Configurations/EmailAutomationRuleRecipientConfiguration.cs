using CRM.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class EmailAutomationRuleRecipientConfiguration : IEntityTypeConfiguration<EmailAutomationRuleRecipient>
{
    public void Configure(EntityTypeBuilder<EmailAutomationRuleRecipient> builder)
    {
        builder.ToTable("EmailAutomationRuleRecipients");

        builder.HasKey(recipient => recipient.Id);

        builder.Property(recipient => recipient.RecipientType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(recipient => recipient.EmailAddress)
            .HasMaxLength(256);

        builder.Property(recipient => recipient.RoleName)
            .HasMaxLength(128);
    }
}

