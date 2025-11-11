using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NULL
BEGIN
    ALTER TABLE dbo.Shipments ADD CustomerId UNIQUEIDENTIFIER NULL;
END");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shipments_CustomerId' AND object_id = OBJECT_ID(N'dbo.Shipments'))
BEGIN
    CREATE INDEX IX_Shipments_CustomerId ON dbo.Shipments(CustomerId);
END");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shipments_Customers_CustomerId')
BEGIN
    ALTER TABLE dbo.Shipments
        ADD CONSTRAINT FK_Shipments_Customers_CustomerId
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id) ON DELETE SET NULL;
END");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CustomsProcesses', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CustomsProcesses', N'DocumentNumber') IS NULL
BEGIN
    ALTER TABLE dbo.CustomsProcesses ADD DocumentNumber NVARCHAR(100) NULL;
END");

            migrationBuilder.Sql(@"
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
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ShipmentStages', N'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ShipmentStages DROP CONSTRAINT FK_ShipmentStages_Shipments_ShipmentId;
    DROP TABLE dbo.ShipmentStages;
END");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Shipments_Customers_CustomerId')
    BEGIN
        ALTER TABLE dbo.Shipments DROP CONSTRAINT FK_Shipments_Customers_CustomerId;
    END

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shipments_CustomerId' AND object_id = OBJECT_ID(N'dbo.Shipments'))
    BEGIN
        DROP INDEX IX_Shipments_CustomerId ON dbo.Shipments;
    END

    IF COL_LENGTH(N'dbo.Shipments', N'CustomerId') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Shipments DROP COLUMN CustomerId;
    END
END");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CustomsProcesses', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.CustomsProcesses', N'DocumentNumber') IS NOT NULL
BEGIN
    ALTER TABLE dbo.CustomsProcesses DROP COLUMN DocumentNumber;
END");
        }
    }
}
