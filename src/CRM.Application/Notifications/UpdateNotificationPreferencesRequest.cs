namespace CRM.Application.Notifications;

public sealed record UpdateNotificationPreferencesRequest(
    bool EmailShipmentUpdates,
    bool EmailPaymentReminders,
    bool EmailWarehouseAlerts,
    bool EmailCustomerInteractions,
    bool EmailSystemAnnouncements,
    bool InAppShipmentUpdates,
    bool InAppPaymentReminders,
    bool InAppWarehouseAlerts,
    bool InAppCustomerInteractions,
    bool InAppSystemAnnouncements);

