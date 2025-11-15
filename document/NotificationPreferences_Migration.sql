-- Migration: Add NotificationPreferences Table
-- Created: 2024-12-XX
-- Description: Creates NotificationPreferences table for user notification settings

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;

-- Create NotificationPreferences table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[NotificationPreferences]') AND type in (N'U'))
BEGIN
    CREATE TABLE [NotificationPreferences] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [EmailShipmentUpdates] bit NOT NULL DEFAULT 1,
        [EmailPaymentReminders] bit NOT NULL DEFAULT 1,
        [EmailWarehouseAlerts] bit NOT NULL DEFAULT 1,
        [EmailCustomerInteractions] bit NOT NULL DEFAULT 0,
        [EmailSystemAnnouncements] bit NOT NULL DEFAULT 1,
        [InAppShipmentUpdates] bit NOT NULL DEFAULT 1,
        [InAppPaymentReminders] bit NOT NULL DEFAULT 1,
        [InAppWarehouseAlerts] bit NOT NULL DEFAULT 1,
        [InAppCustomerInteractions] bit NOT NULL DEFAULT 1,
        [InAppSystemAnnouncements] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([Id])
    );
END;
GO

-- Create unique index on UserId (one preference per user)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_NotificationPreferences_UserId' AND object_id = OBJECT_ID(N'[NotificationPreferences]'))
BEGIN
    CREATE UNIQUE INDEX [IX_NotificationPreferences_UserId] ON [NotificationPreferences] ([UserId]);
END;
GO

-- Create foreign key relationship with AspNetUsers
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_NotificationPreferences_AspNetUsers_UserId')
BEGIN
    ALTER TABLE [NotificationPreferences] 
    ADD CONSTRAINT [FK_NotificationPreferences_AspNetUsers_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;
GO

-- Insert migration history
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241201000000_AddNotificationPreferences')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241201000000_AddNotificationPreferences', N'9.0.0');
END;
GO

COMMIT;
GO

-- Optional: Create default notification preferences for existing users
-- Uncomment the following section if you want to create default preferences for existing users

/*
BEGIN TRANSACTION;

INSERT INTO [NotificationPreferences] (
    [Id],
    [UserId],
    [EmailShipmentUpdates],
    [EmailPaymentReminders],
    [EmailWarehouseAlerts],
    [EmailCustomerInteractions],
    [EmailSystemAnnouncements],
    [InAppShipmentUpdates],
    [InAppPaymentReminders],
    [InAppWarehouseAlerts],
    [InAppCustomerInteractions],
    [InAppSystemAnnouncements],
    [CreatedAt],
    [CreatedBy]
)
SELECT 
    NEWID() AS [Id],
    [Id] AS [UserId],
    1 AS [EmailShipmentUpdates],
    1 AS [EmailPaymentReminders],
    1 AS [EmailWarehouseAlerts],
    0 AS [EmailCustomerInteractions],
    1 AS [EmailSystemAnnouncements],
    1 AS [InAppShipmentUpdates],
    1 AS [InAppPaymentReminders],
    1 AS [InAppWarehouseAlerts],
    1 AS [InAppCustomerInteractions],
    1 AS [InAppSystemAnnouncements],
    GETUTCDATE() AS [CreatedAt],
    'System' AS [CreatedBy]
FROM [AspNetUsers]
WHERE [Id] NOT IN (SELECT [UserId] FROM [NotificationPreferences]);

COMMIT;
GO
*/

