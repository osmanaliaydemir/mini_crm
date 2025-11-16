namespace CRM.Application.Settings;

public sealed record SystemSettingsDto(
    Guid Id,
    string? CompanyName,
    string? CompanyEmail,
    string? CompanyPhone,
    string? CompanyAddress,
    string? CompanyTaxNumber,
    string? CompanyLogoUrl,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    bool SmtpEnableSsl,
    string? SmtpFromEmail,
    string? SmtpFromName,
    int SessionTimeoutMinutes,
    bool EnableEmailNotifications,
    bool EnableSmsNotifications,
    string? SmsProvider,
    int AuditLogRetentionDays,
    int BackupRetentionDays,
    bool EnableAutoBackup,
    string? BackupSchedule,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

