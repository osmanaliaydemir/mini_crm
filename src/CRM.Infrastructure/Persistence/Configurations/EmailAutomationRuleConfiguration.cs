using CRM.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class EmailAutomationRuleConfiguration : IEntityTypeConfiguration<EmailAutomationRule>
{
    public void Configure(EntityTypeBuilder<EmailAutomationRule> builder)
    {
        builder.ToTable("EmailAutomationRules");

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rule => rule.TemplateKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(rule => rule.CronExpression)
            .HasMaxLength(128);

        builder.Property(rule => rule.TimeZoneId)
            .HasMaxLength(128);

        builder.Property(rule => rule.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(rule => rule.ResourceType)
            .HasConversion<int>();

        builder.Property(rule => rule.TriggerType)
            .HasConversion<int>();

        builder.Property(rule => rule.ExecutionType)
            .HasConversion<int>();

        builder.Property(rule => rule.IsActive)
            .HasDefaultValue(true);

        builder.Property(rule => rule.RowVersion)
            .IsRowVersion();

        builder.HasMany(rule => rule.Recipients)
            .WithOne()
            .HasForeignKey(recipient => recipient.RuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

