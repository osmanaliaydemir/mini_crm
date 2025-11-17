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
CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [Locale] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110114251_InitialCreate', N'9.0.10');

CREATE TABLE [LumberVariants] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Species] nvarchar(100) NULL,
    [Grade] nvarchar(50) NULL,
    [StandardVolumeAmount] decimal(18,4) NULL,
    [StandardVolumeUnit] nvarchar(20) NULL,
    [UnitOfMeasure] nvarchar(20) NOT NULL DEFAULT N'm3',
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_LumberVariants] PRIMARY KEY ([Id])
);

CREATE TABLE [Suppliers] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Country] nvarchar(100) NULL,
    [TaxNumber] nvarchar(50) NULL,
    [ContactEmail] nvarchar(200) NULL,
    [ContactPhone] nvarchar(50) NULL,
    [AddressLine] nvarchar(300) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
);

CREATE TABLE [Warehouses] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Location] nvarchar(200) NULL,
    [ContactPerson] nvarchar(150) NULL,
    [ContactPhone] nvarchar(50) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id])
);

CREATE TABLE [Shipments] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierId] uniqueidentifier NOT NULL,
    [ReferenceNumber] nvarchar(100) NOT NULL,
    [ShipmentDate] datetime2 NOT NULL,
    [EstimatedArrival] datetime2 NULL,
    [Status] int NOT NULL,
    [LoadingPort] nvarchar(150) NULL,
    [DischargePort] nvarchar(150) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Shipments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Shipments_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CustomsProcesses] (
    [Id] uniqueidentifier NOT NULL,
    [ShipmentId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [StartedAt] datetime2 NOT NULL,
    [CompletedAt] datetime2 NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CustomsProcesses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomsProcesses_Shipments_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ShipmentItems] (
    [Id] uniqueidentifier NOT NULL,
    [ShipmentId] uniqueidentifier NOT NULL,
    [VariantId] uniqueidentifier NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [Volume] decimal(18,3) NOT NULL,
    CONSTRAINT [PK_ShipmentItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShipmentItems_LumberVariants_VariantId] FOREIGN KEY ([VariantId]) REFERENCES [LumberVariants] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ShipmentItems_Shipments_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ShipmentTransportUnits] (
    [Id] uniqueidentifier NOT NULL,
    [ShipmentId] uniqueidentifier NOT NULL,
    [Mode] int NOT NULL,
    [Identifier] nvarchar(100) NOT NULL,
    [Count] int NOT NULL,
    CONSTRAINT [PK_ShipmentTransportUnits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShipmentTransportUnits_Shipments_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WarehouseUnloadings] (
    [Id] uniqueidentifier NOT NULL,
    [WarehouseId] uniqueidentifier NOT NULL,
    [ShipmentId] uniqueidentifier NOT NULL,
    [TruckPlate] nvarchar(50) NOT NULL,
    [UnloadedAt] datetime2 NOT NULL,
    [UnloadedVolume] decimal(18,3) NOT NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_WarehouseUnloadings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WarehouseUnloadings_Shipments_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WarehouseUnloadings_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_CustomsProcesses_ShipmentId] ON [CustomsProcesses] ([ShipmentId]);

CREATE INDEX [IX_ShipmentItems_ShipmentId] ON [ShipmentItems] ([ShipmentId]);

CREATE INDEX [IX_ShipmentItems_VariantId] ON [ShipmentItems] ([VariantId]);

CREATE INDEX [IX_Shipments_SupplierId] ON [Shipments] ([SupplierId]);

CREATE INDEX [IX_ShipmentTransportUnits_ShipmentId] ON [ShipmentTransportUnits] ([ShipmentId]);

CREATE INDEX [IX_WarehouseUnloadings_ShipmentId] ON [WarehouseUnloadings] ([ShipmentId]);

CREATE INDEX [IX_WarehouseUnloadings_WarehouseId] ON [WarehouseUnloadings] ([WarehouseId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110123929_Sprint1Logistics', N'9.0.10');

CREATE TABLE [Customers] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [LegalName] nvarchar(200) NULL,
    [TaxNumber] nvarchar(50) NULL,
    [Email] nvarchar(200) NULL,
    [Phone] nvarchar(50) NULL,
    [Address] nvarchar(300) NULL,
    [Segment] nvarchar(100) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);

CREATE TABLE [CustomerContacts] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Email] nvarchar(200) NULL,
    [Phone] nvarchar(50) NULL,
    [Position] nvarchar(100) NULL,
    CONSTRAINT [PK_CustomerContacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerContacts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_CustomerContacts_CustomerId] ON [CustomerContacts] ([CustomerId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110125531_Sprint2Customer', N'9.0.10');

CREATE TABLE [CustomerInteractions] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [InteractionDate] datetime2 NOT NULL,
    [InteractionType] nvarchar(100) NOT NULL,
    [Subject] nvarchar(200) NULL,
    [Notes] nvarchar(1000) NULL,
    [RecordedBy] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CustomerInteractions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerInteractions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_CustomerInteractions_CustomerId] ON [CustomerInteractions] ([CustomerId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110130005_Sprint2Interactions', N'9.0.10');

CREATE TABLE [PaymentPlans] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [ShipmentId] uniqueidentifier NOT NULL,
    [PlanType] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(10) NOT NULL DEFAULT N'TRY',
    [StartDate] datetime2 NOT NULL,
    [PeriodicityWeeks] int NOT NULL DEFAULT 1,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_PaymentPlans] PRIMARY KEY ([Id])
);

CREATE TABLE [PaymentInstallments] (
    [Id] uniqueidentifier NOT NULL,
    [PaymentPlanId] uniqueidentifier NOT NULL,
    [InstallmentNumber] int NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [PaidAt] datetime2 NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(10) NOT NULL DEFAULT N'TRY',
    [PaidAmount] decimal(18,2) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_PaymentInstallments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentInstallments_PaymentPlans_PaymentPlanId] FOREIGN KEY ([PaymentPlanId]) REFERENCES [PaymentPlans] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_PaymentInstallments_PaymentPlanId] ON [PaymentInstallments] ([PaymentPlanId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110130954_Sprint2PaymentPlan', N'9.0.10');

CREATE TABLE [CashTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    [TransactionType] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(10) NOT NULL DEFAULT N'TRY',
    [Description] nvarchar(500) NULL,
    [Category] nvarchar(150) NULL,
    [RelatedCustomerId] uniqueidentifier NULL,
    [RelatedShipmentId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CashTransactions] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110131917_Sprint2Cashbox', N'9.0.10');


IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NULL
BEGIN
    ALTER TABLE dbo.Shipments ADD CustomerId UNIQUEIDENTIFIER NULL;
END


IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shipments_CustomerId' AND object_id = OBJECT_ID(N'dbo.Shipments'))
BEGIN
    CREATE INDEX IX_Shipments_CustomerId ON dbo.Shipments(CustomerId);
END


IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shipments_Customers_CustomerId')
BEGIN
    ALTER TABLE dbo.Shipments
        ADD CONSTRAINT FK_Shipments_Customers_CustomerId
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id) ON DELETE SET NULL;
END


IF OBJECT_ID(N'dbo.CustomsProcesses', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CustomsProcesses', N'DocumentNumber') IS NULL
BEGIN
    ALTER TABLE dbo.CustomsProcesses ADD DocumentNumber NVARCHAR(100) NULL;
END


IF OBJECT_ID(N'dbo.ShipmentStages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShipmentStages
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ShipmentId UNIQUEIDENTIFIER NOT NULL,
        Status INT NOT NULL,
        StartedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2 NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(256) NULL,
        LastModifiedAt DATETIME2 NULL,
        LastModifiedBy NVARCHAR(256) NULL
    );

    CREATE INDEX IX_ShipmentStages_ShipmentId ON dbo.ShipmentStages(ShipmentId);

    ALTER TABLE dbo.ShipmentStages
        ADD CONSTRAINT FK_ShipmentStages_Shipments_ShipmentId
        FOREIGN KEY (ShipmentId) REFERENCES dbo.Shipments(Id) ON DELETE CASCADE;
END

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110231817_AddShipmentStages', N'9.0.10');

CREATE TABLE [NotificationPreferences] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [EmailShipmentUpdates] bit NOT NULL DEFAULT CAST(1 AS bit),
    [EmailPaymentReminders] bit NOT NULL DEFAULT CAST(1 AS bit),
    [EmailWarehouseAlerts] bit NOT NULL DEFAULT CAST(1 AS bit),
    [EmailCustomerInteractions] bit NOT NULL DEFAULT CAST(0 AS bit),
    [EmailSystemAnnouncements] bit NOT NULL DEFAULT CAST(1 AS bit),
    [InAppShipmentUpdates] bit NOT NULL DEFAULT CAST(1 AS bit),
    [InAppPaymentReminders] bit NOT NULL DEFAULT CAST(1 AS bit),
    [InAppWarehouseAlerts] bit NOT NULL DEFAULT CAST(1 AS bit),
    [InAppCustomerInteractions] bit NOT NULL DEFAULT CAST(1 AS bit),
    [InAppSystemAnnouncements] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NotificationPreferences_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_NotificationPreferences_UserId] ON [NotificationPreferences] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251115145402_AddNotificationPreferences', N'9.0.10');


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WarehouseUnloadings') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [WarehouseUnloadings] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Warehouses') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [Warehouses] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [Suppliers] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ShipmentStages') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [ShipmentStages] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shipments') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [Shipments] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentPlans') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [PaymentPlans] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentInstallments') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [PaymentInstallments] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NotificationPreferences') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [NotificationPreferences] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LumberVariants') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [LumberVariants] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CustomsProcesses') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [CustomsProcesses] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [Customers] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CustomerInteractions') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [CustomerInteractions] ADD [RowVersion] rowversion NOT NULL;
                    END
                


                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CashTransactions') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [CashTransactions] ADD [RowVersion] rowversion NOT NULL;
                    END
                

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251115223933_AddRowVersionConcurrencyControl', N'9.0.10');


                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [AuditLogs] (
                        [Id] uniqueidentifier NOT NULL,
                        [EntityType] nvarchar(100) NOT NULL,
                        [EntityId] uniqueidentifier NOT NULL,
                        [Action] nvarchar(50) NOT NULL,
                        [UserId] nvarchar(450) NULL,
                        [UserName] nvarchar(256) NULL,
                        [Changes] nvarchar(max) NULL,
                        [IpAddress] nvarchar(45) NULL,
                        [UserAgent] nvarchar(500) NULL,
                        [Timestamp] datetime2 NOT NULL,
                        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
                    );
                END
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Action' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Action] ON [AuditLogs]([Action] ASC);
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityId] ON [AuditLogs]([EntityId] ASC);
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType] ON [AuditLogs]([EntityType] ASC);
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType_EntityId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs]([EntityType] ASC, [EntityId] ASC);
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Timestamp' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs]([Timestamp] DESC);
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] ON [AuditLogs]([UserId] ASC);
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116215109_AddAuditLogs', N'9.0.10');

CREATE TABLE [SystemSettings] (
    [Id] uniqueidentifier NOT NULL,
    [CompanyName] nvarchar(200) NULL,
    [CompanyEmail] nvarchar(200) NULL,
    [CompanyPhone] nvarchar(50) NULL,
    [CompanyAddress] nvarchar(500) NULL,
    [CompanyTaxNumber] nvarchar(50) NULL,
    [CompanyLogoUrl] nvarchar(500) NULL,
    [SmtpHost] nvarchar(200) NULL,
    [SmtpPort] int NULL,
    [SmtpUsername] nvarchar(200) NULL,
    [SmtpPassword] nvarchar(500) NULL,
    [SmtpEnableSsl] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SmtpFromEmail] nvarchar(200) NULL,
    [SmtpFromName] nvarchar(200) NULL,
    [SessionTimeoutMinutes] int NOT NULL DEFAULT 60,
    [EnableEmailNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
    [EnableSmsNotifications] bit NOT NULL DEFAULT CAST(0 AS bit),
    [SmsProvider] nvarchar(100) NULL,
    [SmsApiKey] nvarchar(500) NULL,
    [AuditLogRetentionDays] int NOT NULL DEFAULT 90,
    [BackupRetentionDays] int NOT NULL DEFAULT 30,
    [EnableAutoBackup] bit NOT NULL DEFAULT CAST(0 AS bit),
    [BackupSchedule] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(100) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116231527_AddSystemSettings', N'9.0.10');


                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tasks]') AND type in (N'U'))
                BEGIN
                    -- Tasks table exists, rename it to TaskDb
                    EXEC sp_rename 'Tasks', 'TaskDb';
                    
                    -- Rename indexes
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tasks_AssignedToUserId' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        EXEC sp_rename 'TaskDb.IX_Tasks_AssignedToUserId', 'IX_TaskDb_AssignedToUserId', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tasks_Status' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        EXEC sp_rename 'TaskDb.IX_Tasks_Status', 'IX_TaskDb_Status', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tasks_DueDate' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        EXEC sp_rename 'TaskDb.IX_Tasks_DueDate', 'IX_TaskDb_DueDate', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tasks_CreatedAt' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        EXEC sp_rename 'TaskDb.IX_Tasks_CreatedAt', 'IX_TaskDb_CreatedAt', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tasks_RelatedEntity' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        EXEC sp_rename 'TaskDb.IX_Tasks_RelatedEntity', 'IX_TaskDb_RelatedEntity', 'INDEX';
                    
                    -- Rename primary key
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_Tasks]') AND type in (N'PK'))
                        EXEC sp_rename 'PK_Tasks', 'PK_TaskDb', 'OBJECT';
                    
                    -- Rename foreign key constraint
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Tasks_AspNetUsers_AssignedToUserId')
                        EXEC sp_rename 'FK_Tasks_AspNetUsers_AssignedToUserId', 'FK_TaskDb_AspNetUsers_AssignedToUserId', 'OBJECT';
                END
                ELSE IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaskDb]') AND type in (N'U'))
                BEGIN
                    -- Neither Tasks nor TaskDb exists, create new TaskDb table
                    CREATE TABLE [dbo].[TaskDb](
                        [Id] [uniqueidentifier] NOT NULL,
                        [Title] [nvarchar](200) NOT NULL,
                        [Description] [nvarchar](1000) NULL,
                        [Status] [int] NOT NULL DEFAULT 0,
                        [Priority] [int] NOT NULL DEFAULT 1,
                        [DueDate] [datetime2](7) NULL,
                        [AssignedToUserId] [uniqueidentifier] NULL,
                        [RelatedEntityType] [nvarchar](100) NULL,
                        [RelatedEntityId] [uniqueidentifier] NULL,
                        [CreatedAt] [datetime2](7) NOT NULL,
                        [CreatedBy] [nvarchar](100) NULL,
                        [LastModifiedAt] [datetime2](7) NULL,
                        [LastModifiedBy] [nvarchar](100) NULL,
                        [RowVersion] [rowversion] NOT NULL,
                        CONSTRAINT [PK_TaskDb] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    
                    -- Create indexes
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_AssignedToUserId' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        CREATE NONCLUSTERED INDEX [IX_TaskDb_AssignedToUserId] ON [dbo].[TaskDb] ([AssignedToUserId] ASC);
                    
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_Status' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        CREATE NONCLUSTERED INDEX [IX_TaskDb_Status] ON [dbo].[TaskDb] ([Status] ASC);
                    
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_DueDate' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        CREATE NONCLUSTERED INDEX [IX_TaskDb_DueDate] ON [dbo].[TaskDb] ([DueDate] ASC);
                    
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_CreatedAt' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        CREATE NONCLUSTERED INDEX [IX_TaskDb_CreatedAt] ON [dbo].[TaskDb] ([CreatedAt] DESC);
                    
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_RelatedEntity' AND object_id = OBJECT_ID('dbo.TaskDb'))
                        CREATE NONCLUSTERED INDEX [IX_TaskDb_RelatedEntity] ON [dbo].[TaskDb] ([RelatedEntityType] ASC, [RelatedEntityId] ASC);
                    
                    -- Create foreign key if AspNetUsers exists
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TaskDb_AspNetUsers_AssignedToUserId')
                        BEGIN
                            ALTER TABLE [dbo].[TaskDb]
                            ADD CONSTRAINT [FK_TaskDb_AspNetUsers_AssignedToUserId]
                            FOREIGN KEY([AssignedToUserId])
                            REFERENCES [dbo].[AspNetUsers] ([Id])
                            ON DELETE SET NULL;
                        END
                    END
                END
                -- If TaskDb already exists, do nothing (idempotent)
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251117005323_RenameTaskToTaskDb', N'9.0.10');

CREATE TABLE [EmailAutomationRules] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [ResourceType] int NOT NULL,
    [TriggerType] int NOT NULL,
    [ExecutionType] int NOT NULL,
    [TemplateKey] nvarchar(128) NOT NULL,
    [CronExpression] nvarchar(128) NULL,
    [TimeZoneId] nvarchar(128) NULL,
    [RelatedEntityId] uniqueidentifier NULL,
    [Metadata] nvarchar(max) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_EmailAutomationRules] PRIMARY KEY ([Id])
);

CREATE TABLE [EmailAutomationRuleRecipients] (
    [Id] uniqueidentifier NOT NULL,
    [RuleId] uniqueidentifier NOT NULL,
    [RecipientType] int NOT NULL,
    [UserId] uniqueidentifier NULL,
    [EmailAddress] nvarchar(256) NULL,
    CONSTRAINT [PK_EmailAutomationRuleRecipients] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmailAutomationRuleRecipients_EmailAutomationRules_RuleId] FOREIGN KEY ([RuleId]) REFERENCES [EmailAutomationRules] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_EmailAutomationRuleRecipients_RuleId] ON [EmailAutomationRuleRecipients] ([RuleId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251117120723_AddEmailAutomationRules', N'9.0.10');

ALTER TABLE [EmailAutomationRuleRecipients] ADD [RoleName] nvarchar(128) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251117142036_AddEmailAutomationRoleRecipients', N'9.0.10');

COMMIT;
GO

