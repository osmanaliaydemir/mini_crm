using CRM.Domain.Abstractions;

namespace CRM.Domain.Notifications;

public class NotificationPreferences : Entity<Guid>, IAuditableEntity
{
    private NotificationPreferences()
    {
    }

    public NotificationPreferences(
        Guid id,
        Guid userId,
        bool emailShipmentUpdates = true,
        bool emailPaymentReminders = true,
        bool emailWarehouseAlerts = true,
        bool emailCustomerInteractions = false,
        bool emailSystemAnnouncements = true,
        bool inAppShipmentUpdates = true,
        bool inAppPaymentReminders = true,
        bool inAppWarehouseAlerts = true,
        bool inAppCustomerInteractions = true,
        bool inAppSystemAnnouncements = true)
    {
        Id = id;
        UserId = userId;
        EmailShipmentUpdates = emailShipmentUpdates;
        EmailPaymentReminders = emailPaymentReminders;
        EmailWarehouseAlerts = emailWarehouseAlerts;
        EmailCustomerInteractions = emailCustomerInteractions;
        EmailSystemAnnouncements = emailSystemAnnouncements;
        InAppShipmentUpdates = inAppShipmentUpdates;
        InAppPaymentReminders = inAppPaymentReminders;
        InAppWarehouseAlerts = inAppWarehouseAlerts;
        InAppCustomerInteractions = inAppCustomerInteractions;
        InAppSystemAnnouncements = inAppSystemAnnouncements;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    // Email Notifications
    public bool EmailShipmentUpdates { get; private set; }
    public bool EmailPaymentReminders { get; private set; }
    public bool EmailWarehouseAlerts { get; private set; }
    public bool EmailCustomerInteractions { get; private set; }
    public bool EmailSystemAnnouncements { get; private set; }

    // In-App Notifications
    public bool InAppShipmentUpdates { get; private set; }
    public bool InAppPaymentReminders { get; private set; }
    public bool InAppWarehouseAlerts { get; private set; }
    public bool InAppCustomerInteractions { get; private set; }
    public bool InAppSystemAnnouncements { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Update(
        bool emailShipmentUpdates,
        bool emailPaymentReminders,
        bool emailWarehouseAlerts,
        bool emailCustomerInteractions,
        bool emailSystemAnnouncements,
        bool inAppShipmentUpdates,
        bool inAppPaymentReminders,
        bool inAppWarehouseAlerts,
        bool inAppCustomerInteractions,
        bool inAppSystemAnnouncements)
    {
        EmailShipmentUpdates = emailShipmentUpdates;
        EmailPaymentReminders = emailPaymentReminders;
        EmailWarehouseAlerts = emailWarehouseAlerts;
        EmailCustomerInteractions = emailCustomerInteractions;
        EmailSystemAnnouncements = emailSystemAnnouncements;
        InAppShipmentUpdates = inAppShipmentUpdates;
        InAppPaymentReminders = inAppPaymentReminders;
        InAppWarehouseAlerts = inAppWarehouseAlerts;
        InAppCustomerInteractions = inAppCustomerInteractions;
        InAppSystemAnnouncements = inAppSystemAnnouncements;
        LastModifiedAt = DateTime.UtcNow;
    }
}

