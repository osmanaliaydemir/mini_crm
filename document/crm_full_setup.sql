IF DB_ID(N'CRMDb') IS NULL
BEGIN
    PRINT 'Creating database CRMDb';
    CREATE DATABASE [CRMDb];
END;
GO

USE [CRMDb];
GO

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

COMMIT;
GO
