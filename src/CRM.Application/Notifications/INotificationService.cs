namespace CRM.Application.Notifications;

public interface INotificationService
{
    Task<NotificationPreferencesDto?> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
}

