namespace CRM.Application.Settings;

public interface ISystemSettingsService
{
    Task<SystemSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task UpdateCompanyInfoAsync(UpdateCompanyInfoRequest request, CancellationToken cancellationToken = default);
    Task UpdateSmtpSettingsAsync(UpdateSmtpSettingsRequest request, CancellationToken cancellationToken = default);
    Task UpdateSystemSettingsAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken = default);
    Task UpdateMaintenanceSettingsAsync(UpdateMaintenanceSettingsRequest request, CancellationToken cancellationToken = default);
}

