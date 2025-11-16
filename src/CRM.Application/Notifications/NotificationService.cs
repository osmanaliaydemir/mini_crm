using CRM.Application.Common;
using CRM.Application.Common.Exceptions;
using CRM.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationPreferencesDto?> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _context.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            return null;
        }

        return new NotificationPreferencesDto(
            preferences.Id,
            preferences.UserId,
            preferences.EmailShipmentUpdates,
            preferences.EmailPaymentReminders,
            preferences.EmailWarehouseAlerts,
            preferences.EmailCustomerInteractions,
            preferences.EmailSystemAnnouncements,
            preferences.InAppShipmentUpdates,
            preferences.InAppPaymentReminders,
            preferences.InAppWarehouseAlerts,
            preferences.InAppCustomerInteractions,
            preferences.InAppSystemAnnouncements);
    }

    public async Task UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var preferences = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            // Yeni kayıt oluştur
            preferences = new NotificationPreferences(
                Guid.NewGuid(),
                userId,
                request.EmailShipmentUpdates,
                request.EmailPaymentReminders,
                request.EmailWarehouseAlerts,
                request.EmailCustomerInteractions,
                request.EmailSystemAnnouncements,
                request.InAppShipmentUpdates,
                request.InAppPaymentReminders,
                request.InAppWarehouseAlerts,
                request.InAppCustomerInteractions,
                request.InAppSystemAnnouncements);

            await _context.NotificationPreferences.AddAsync(preferences, cancellationToken);
        }
        else
        {
            // Mevcut kaydı güncelle
            preferences.Update(
                request.EmailShipmentUpdates,
                request.EmailPaymentReminders,
                request.EmailWarehouseAlerts,
                request.EmailCustomerInteractions,
                request.EmailSystemAnnouncements,
                request.InAppShipmentUpdates,
                request.InAppPaymentReminders,
                request.InAppWarehouseAlerts,
                request.InAppCustomerInteractions,
                request.InAppSystemAnnouncements);

            _context.NotificationPreferences.Update(preferences);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

