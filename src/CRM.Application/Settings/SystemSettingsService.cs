using CRM.Application.Common;
using CRM.Application.Common.Exceptions;
using CRM.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Settings;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private const string DefaultSettingsId = "00000000-0000-0000-0000-000000000001";

    public SystemSettingsService(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<SystemSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settingsId = Guid.Parse(DefaultSettingsId);
        var settings = await _context.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == settingsId, cancellationToken);

        if (settings == null)
        {
            return null;
        }

        return new SystemSettingsDto(
            settings.Id,
            settings.CompanyName,
            settings.CompanyEmail,
            settings.CompanyPhone,
            settings.CompanyAddress,
            settings.CompanyTaxNumber,
            settings.CompanyLogoUrl,
            settings.SmtpHost,
            settings.SmtpPort,
            settings.SmtpUsername,
            settings.SmtpEnableSsl,
            settings.SmtpFromEmail,
            settings.SmtpFromName,
            settings.SessionTimeoutMinutes,
            settings.EnableEmailNotifications,
            settings.EnableSmsNotifications,
            settings.SmsProvider,
            settings.AuditLogRetentionDays,
            settings.BackupRetentionDays,
            settings.EnableAutoBackup,
            settings.BackupSchedule,
            settings.CreatedAt,
            settings.LastModifiedAt);
    }

    public async Task UpdateCompanyInfoAsync(UpdateCompanyInfoRequest request, CancellationToken cancellationToken = default)
    {
        var settingsId = Guid.Parse(DefaultSettingsId);
        var settings = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == settingsId, cancellationToken);

        if (settings == null)
        {
            settings = new SystemSettings(settingsId);
            await _context.SystemSettings.AddAsync(settings, cancellationToken);
        }

        settings.UpdateCompanyInfo(
            request.CompanyName,
            request.CompanyEmail,
            request.CompanyPhone,
            request.CompanyAddress,
            request.CompanyTaxNumber,
            request.CompanyLogoUrl);

        // Entity is already tracked, no need to call Update()
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSmtpSettingsAsync(UpdateSmtpSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settingsId = Guid.Parse(DefaultSettingsId);
        var settings = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == settingsId, cancellationToken);

        if (settings == null)
        {
            settings = new SystemSettings(settingsId);
            await _context.SystemSettings.AddAsync(settings, cancellationToken);
        }

        settings.UpdateSmtpSettings(
            request.SmtpHost,
            request.SmtpPort,
            request.SmtpUsername,
            request.SmtpPassword,
            request.SmtpEnableSsl,
            request.SmtpFromEmail,
            request.SmtpFromName);

        // Entity is already tracked, no need to call Update()
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSystemSettingsAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settingsId = Guid.Parse(DefaultSettingsId);
        var settings = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == settingsId, cancellationToken);

        if (settings == null)
        {
            settings = new SystemSettings(settingsId);
            await _context.SystemSettings.AddAsync(settings, cancellationToken);
        }

        settings.UpdateSystemSettings(
            request.SessionTimeoutMinutes,
            request.EnableEmailNotifications,
            request.EnableSmsNotifications,
            request.SmsProvider,
            request.SmsApiKey);

        // Entity is already tracked, no need to call Update()
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMaintenanceSettingsAsync(UpdateMaintenanceSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settingsId = Guid.Parse(DefaultSettingsId);
        var settings = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == settingsId, cancellationToken);

        if (settings == null)
        {
            settings = new SystemSettings(settingsId);
            await _context.SystemSettings.AddAsync(settings, cancellationToken);
        }

        settings.UpdateMaintenanceSettings(
            request.AuditLogRetentionDays,
            request.BackupRetentionDays,
            request.EnableAutoBackup,
            request.BackupSchedule);

        // Entity is already tracked, no need to call Update()
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

