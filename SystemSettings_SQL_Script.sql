-- =============================================
-- SystemSettings Table Creation Script
-- =============================================

-- Create SystemSettings Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemSettings] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [CompanyName] NVARCHAR(200) NULL,
        [CompanyEmail] NVARCHAR(200) NULL,
        [CompanyPhone] NVARCHAR(50) NULL,
        [CompanyAddress] NVARCHAR(500) NULL,
        [CompanyTaxNumber] NVARCHAR(50) NULL,
        [CompanyLogoUrl] NVARCHAR(500) NULL,
        [SmtpHost] NVARCHAR(200) NULL,
        [SmtpPort] INT NULL,
        [SmtpUsername] NVARCHAR(200) NULL,
        [SmtpPassword] NVARCHAR(500) NULL,
        [SmtpEnableSsl] BIT NOT NULL DEFAULT 1,
        [SmtpFromEmail] NVARCHAR(200) NULL,
        [SmtpFromName] NVARCHAR(200) NULL,
        [SessionTimeoutMinutes] INT NOT NULL DEFAULT 60,
        [EnableEmailNotifications] BIT NOT NULL DEFAULT 1,
        [EnableSmsNotifications] BIT NOT NULL DEFAULT 0,
        [SmsProvider] NVARCHAR(100) NULL,
        [SmsApiKey] NVARCHAR(500) NULL,
        [AuditLogRetentionDays] INT NOT NULL DEFAULT 90,
        [BackupRetentionDays] INT NOT NULL DEFAULT 30,
        [EnableAutoBackup] BIT NOT NULL DEFAULT 0,
        [BackupSchedule] NVARCHAR(100) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedAt] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'SystemSettings table created successfully.';
END
ELSE
BEGIN
    PRINT 'SystemSettings table already exists.';
END
GO

-- Insert Default SystemSettings Record (Singleton)
-- Using a fixed GUID for singleton pattern
DECLARE @DefaultSettingsId UNIQUEIDENTIFIER = 'A0A0A0A0-B0B0-C0C0-D0D0-E0E0E0E0E0E0';

IF NOT EXISTS (SELECT 1 FROM [dbo].[SystemSettings] WHERE [Id] = @DefaultSettingsId)
BEGIN
    INSERT INTO [dbo].[SystemSettings] (
        [Id],
        [CompanyName],
        [CompanyEmail],
        [CompanyPhone],
        [CompanyAddress],
        [CompanyTaxNumber],
        [CompanyLogoUrl],
        [SmtpHost],
        [SmtpPort],
        [SmtpUsername],
        [SmtpPassword],
        [SmtpEnableSsl],
        [SmtpFromEmail],
        [SmtpFromName],
        [SessionTimeoutMinutes],
        [EnableEmailNotifications],
        [EnableSmsNotifications],
        [SmsProvider],
        [SmsApiKey],
        [AuditLogRetentionDays],
        [BackupRetentionDays],
        [EnableAutoBackup],
        [BackupSchedule],
        [CreatedAt]
    )
    VALUES (
        @DefaultSettingsId,
        NULL, -- CompanyName
        NULL, -- CompanyEmail
        NULL, -- CompanyPhone
        NULL, -- CompanyAddress
        NULL, -- CompanyTaxNumber
        NULL, -- CompanyLogoUrl
        NULL, -- SmtpHost
        NULL, -- SmtpPort
        NULL, -- SmtpUsername
        NULL, -- SmtpPassword
        1,    -- SmtpEnableSsl (default: true)
        NULL, -- SmtpFromEmail
        NULL, -- SmtpFromName
        60,   -- SessionTimeoutMinutes (default: 60)
        1,    -- EnableEmailNotifications (default: true)
        0,    -- EnableSmsNotifications (default: false)
        NULL, -- SmsProvider
        NULL, -- SmsApiKey
        90,   -- AuditLogRetentionDays (default: 90)
        30,   -- BackupRetentionDays (default: 30)
        0,    -- EnableAutoBackup (default: false)
        NULL, -- BackupSchedule
        GETUTCDATE() -- CreatedAt
    );
    
    PRINT 'Default SystemSettings record inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Default SystemSettings record already exists.';
END
GO

-- =============================================
-- Script completed successfully
-- =============================================

