using CRM.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        // Email Notifications
        builder.Property(x => x.EmailShipmentUpdates)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.EmailPaymentReminders)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.EmailWarehouseAlerts)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.EmailCustomerInteractions)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.EmailSystemAnnouncements)
            .IsRequired()
            .HasDefaultValue(true);

        // In-App Notifications
        builder.Property(x => x.InAppShipmentUpdates)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.InAppPaymentReminders)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.InAppWarehouseAlerts)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.InAppCustomerInteractions)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.InAppSystemAnnouncements)
            .IsRequired()
            .HasDefaultValue(true);

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

        // Relationship with ApplicationUser (configured via foreign key only)
        builder.HasOne<CRM.Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: One preference per user
        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}

