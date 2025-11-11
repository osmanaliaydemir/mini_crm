BEGIN TRANSACTION;

IF COL_LENGTH('Shipments', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE dbo.Shipments
        ADD CustomerId UNIQUEIDENTIFIER NULL;

    CREATE INDEX IX_Shipments_CustomerId
        ON dbo.Shipments(CustomerId);

    ALTER TABLE dbo.Shipments
        ADD CONSTRAINT FK_Shipments_Customers_CustomerId
            FOREIGN KEY (CustomerId)
            REFERENCES dbo.Customers(Id)
            ON DELETE SET NULL;
END;

IF COL_LENGTH('CustomsProcesses', 'DocumentNumber') IS NULL
BEGIN
    ALTER TABLE dbo.CustomsProcesses
        ADD DocumentNumber NVARCHAR(100) NULL;
END;

IF OBJECT_ID('dbo.ShipmentStages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShipmentStages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShipmentStages PRIMARY KEY,
        ShipmentId UNIQUEIDENTIFIER NOT NULL,
        Status INT NOT NULL,
        StartedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2 NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ShipmentStages_CreatedAt DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(256) NULL,
        LastModifiedAt DATETIME2 NULL,
        LastModifiedBy NVARCHAR(256) NULL
    );

    CREATE INDEX IX_ShipmentStages_ShipmentId
        ON dbo.ShipmentStages(ShipmentId);

    ALTER TABLE dbo.ShipmentStages
        ADD CONSTRAINT FK_ShipmentStages_Shipments_ShipmentId
            FOREIGN KEY (ShipmentId)
            REFERENCES dbo.Shipments(Id)
            ON DELETE CASCADE;
END;

-- Map legacy shipment statuses to the new workflow stages
UPDATE dbo.Shipments
SET Status =
    CASE Status
        WHEN 0 THEN 0   -- Draft -> Ordered
        WHEN 1 THEN 2   -- InTransit -> Departed
        WHEN 2 THEN 6   -- Customs -> InCustoms
        WHEN 3 THEN 9   -- Delivered -> DeliveredToDealer
        WHEN 4 THEN 10  -- Cancelled -> Cancelled
        ELSE Status
    END;

COMMIT TRANSACTION;


