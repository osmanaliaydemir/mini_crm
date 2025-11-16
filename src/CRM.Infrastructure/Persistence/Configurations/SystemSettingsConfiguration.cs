using CRM.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(x => x.Id);

        // Company Information
        builder.Property(x => x.CompanyName)
            .HasMaxLength(200);

        builder.Property(x => x.CompanyEmail)
            .HasMaxLength(200);

        builder.Property(x => x.CompanyPhone)
            .HasMaxLength(50);

        builder.Property(x => x.CompanyAddress)
            .HasMaxLength(500);

        builder.Property(x => x.CompanyTaxNumber)
            .HasMaxLength(50);

        builder.Property(x => x.CompanyLogoUrl)
            .HasMaxLength(500);

        // SMTP Settings
        builder.Property(x => x.SmtpHost)
            .HasMaxLength(200);

        builder.Property(x => x.SmtpPort);

        builder.Property(x => x.SmtpUsername)
            .HasMaxLength(200);

        builder.Property(x => x.SmtpPassword)
            .HasMaxLength(500);

        builder.Property(x => x.SmtpEnableSsl)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmtpFromEmail)
            .HasMaxLength(200);

        builder.Property(x => x.SmtpFromName)
            .HasMaxLength(200);

        // System Settings
        builder.Property(x => x.SessionTimeoutMinutes)
            .IsRequired()
            .HasDefaultValue(60);

        builder.Property(x => x.EnableEmailNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.EnableSmsNotifications)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SmsProvider)
            .HasMaxLength(100);

        builder.Property(x => x.SmsApiKey)
            .HasMaxLength(500);

        // Maintenance Settings
        builder.Property(x => x.AuditLogRetentionDays)
            .IsRequired()
            .HasDefaultValue(90);

        builder.Property(x => x.BackupRetentionDays)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(x => x.EnableAutoBackup)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.BackupSchedule)
            .HasMaxLength(100);

        // Auditable fields
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

