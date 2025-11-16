namespace CRM.Application.Settings;

public sealed record UpdateCompanyInfoRequest(
    string? CompanyName,
    string? CompanyEmail,
    string? CompanyPhone,
    string? CompanyAddress,
    string? CompanyTaxNumber,
    string? CompanyLogoUrl);

public sealed record UpdateSmtpSettingsRequest(
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    bool SmtpEnableSsl,
    string? SmtpFromEmail,
    string? SmtpFromName);

public sealed record UpdateSystemSettingsRequest(
    int SessionTimeoutMinutes,
    bool EnableEmailNotifications,
    bool EnableSmsNotifications,
    string? SmsProvider,
    string? SmsApiKey);

public sealed record UpdateMaintenanceSettingsRequest(
    int AuditLogRetentionDays,
    int BackupRetentionDays,
    bool EnableAutoBackup,
    string? BackupSchedule);

