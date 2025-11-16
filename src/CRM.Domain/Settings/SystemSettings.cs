using CRM.Domain.Abstractions;

namespace CRM.Domain.Settings;

public class SystemSettings : Entity<Guid>, IAuditableEntity
{
    private SystemSettings()
    {
    }

    public SystemSettings(
        Guid id,
        string? companyName = null,
        string? companyEmail = null,
        string? companyPhone = null,
        string? companyAddress = null,
        string? companyTaxNumber = null,
        string? companyLogoUrl = null,
        string? smtpHost = null,
        int? smtpPort = null,
        string? smtpUsername = null,
        string? smtpPassword = null,
        bool smtpEnableSsl = true,
        string? smtpFromEmail = null,
        string? smtpFromName = null,
        int sessionTimeoutMinutes = 60,
        bool enableEmailNotifications = true,
        bool enableSmsNotifications = false,
        string? smsProvider = null,
        string? smsApiKey = null,
        int auditLogRetentionDays = 90,
        int backupRetentionDays = 30,
        bool enableAutoBackup = false,
        string? backupSchedule = null)
    {
        Id = id;
        CompanyName = companyName;
        CompanyEmail = companyEmail;
        CompanyPhone = companyPhone;
        CompanyAddress = companyAddress;
        CompanyTaxNumber = companyTaxNumber;
        CompanyLogoUrl = companyLogoUrl;
        SmtpHost = smtpHost;
        SmtpPort = smtpPort;
        SmtpUsername = smtpUsername;
        SmtpPassword = smtpPassword;
        SmtpEnableSsl = smtpEnableSsl;
        SmtpFromEmail = smtpFromEmail;
        SmtpFromName = smtpFromName;
        SessionTimeoutMinutes = sessionTimeoutMinutes;
        EnableEmailNotifications = enableEmailNotifications;
        EnableSmsNotifications = enableSmsNotifications;
        SmsProvider = smsProvider;
        SmsApiKey = smsApiKey;
        AuditLogRetentionDays = auditLogRetentionDays;
        BackupRetentionDays = backupRetentionDays;
        EnableAutoBackup = enableAutoBackup;
        BackupSchedule = backupSchedule;
        CreatedAt = DateTime.UtcNow;
    }

    // Company Information
    public string? CompanyName { get; private set; }
    public string? CompanyEmail { get; private set; }
    public string? CompanyPhone { get; private set; }
    public string? CompanyAddress { get; private set; }
    public string? CompanyTaxNumber { get; private set; }
    public string? CompanyLogoUrl { get; private set; }

    // SMTP Settings
    public string? SmtpHost { get; private set; }
    public int? SmtpPort { get; private set; }
    public string? SmtpUsername { get; private set; }
    public string? SmtpPassword { get; private set; }
    public bool SmtpEnableSsl { get; private set; }
    public string? SmtpFromEmail { get; private set; }
    public string? SmtpFromName { get; private set; }

    // System Settings
    public int SessionTimeoutMinutes { get; private set; }
    public bool EnableEmailNotifications { get; private set; }
    public bool EnableSmsNotifications { get; private set; }
    public string? SmsProvider { get; private set; }
    public string? SmsApiKey { get; private set; }

    // Maintenance Settings
    public int AuditLogRetentionDays { get; private set; }
    public int BackupRetentionDays { get; private set; }
    public bool EnableAutoBackup { get; private set; }
    public string? BackupSchedule { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void UpdateCompanyInfo(
        string? companyName,
        string? companyEmail,
        string? companyPhone,
        string? companyAddress,
        string? companyTaxNumber,
        string? companyLogoUrl)
    {
        CompanyName = companyName;
        CompanyEmail = companyEmail;
        CompanyPhone = companyPhone;
        CompanyAddress = companyAddress;
        CompanyTaxNumber = companyTaxNumber;
        CompanyLogoUrl = companyLogoUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSmtpSettings(
        string? smtpHost,
        int? smtpPort,
        string? smtpUsername,
        string? smtpPassword,
        bool smtpEnableSsl,
        string? smtpFromEmail,
        string? smtpFromName)
    {
        SmtpHost = smtpHost;
        SmtpPort = smtpPort;
        SmtpUsername = smtpUsername;
        SmtpPassword = smtpPassword;
        SmtpEnableSsl = smtpEnableSsl;
        SmtpFromEmail = smtpFromEmail;
        SmtpFromName = smtpFromName;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSystemSettings(
        int sessionTimeoutMinutes,
        bool enableEmailNotifications,
        bool enableSmsNotifications,
        string? smsProvider,
        string? smsApiKey)
    {
        SessionTimeoutMinutes = sessionTimeoutMinutes;
        EnableEmailNotifications = enableEmailNotifications;
        EnableSmsNotifications = enableSmsNotifications;
        SmsProvider = smsProvider;
        SmsApiKey = smsApiKey;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateMaintenanceSettings(
        int auditLogRetentionDays,
        int backupRetentionDays,
        bool enableAutoBackup,
        string? backupSchedule)
    {
        AuditLogRetentionDays = auditLogRetentionDays;
        BackupRetentionDays = backupRetentionDays;
        EnableAutoBackup = enableAutoBackup;
        BackupSchedule = backupSchedule;
        LastModifiedAt = DateTime.UtcNow;
    }
}

