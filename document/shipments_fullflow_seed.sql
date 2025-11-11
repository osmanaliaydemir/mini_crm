BEGIN TRANSACTION;

DECLARE @now DATETIME;
SET @now = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers)
BEGIN
    INSERT INTO dbo.Suppliers
        (Id, Name, Country, TaxNumber, ContactEmail, ContactPhone, AddressLine, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    VALUES
        (NEWID(), N'Volga Timber Export', N'Russia', N'RU123456789', N'ops@volgatimber.com', N'+7 812 555 1212', N'Port Terminal District, St. Petersburg', NULL, @now, NULL, NULL, NULL),
        (NEWID(), N'Nordic Wood Alliance', N'Finland', N'FI987654321', N'support@nordicwood.fi', N'+358 9 123 4567', N'Vantaa Logistics Park, Helsinki', NULL, @now, NULL, NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    INSERT INTO dbo.Customers
        (Id, Name, LegalName, TaxNumber, Email, Phone, Address, Segment, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    VALUES
        (NEWID(), N'GMS Bayii İstanbul', N'GMS İstanbul Kereste A.Ş.', N'TR5566778899', N'logistics@gms-istanbul.com', N'+90 212 555 3344', N'İkitelli OSB, İstanbul', N'Bayi', NULL, @now, NULL, NULL, NULL),
        (NEWID(), N'Anadolu İnşaat', N'Anadolu İnşaat ve Taahhüt Ltd.', N'TR4433221100', N'satinalma@anadoluinsaat.com', N'+90 312 444 5566', N'Ostim Sanayi Sitesi, Ankara', N'Proje', NULL, @now, NULL, NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.LumberVariants)
BEGIN
    INSERT INTO dbo.LumberVariants
        (Id, Name, Species, Grade, StandardVolumeAmount, StandardVolumeUnit, UnitOfMeasure, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    VALUES
        (NEWID(), N'KVH 100x200', N'Spruce', N'A', 0.5000, N'm3', N'm3', NULL, @now, NULL, NULL, NULL),
        (NEWID(), N'Laminated Beam 120x240', N'Pine', N'B', NULL, NULL, N'm3', NULL, @now, NULL, NULL, NULL);
END;

DECLARE @supplierId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id
    FROM dbo.Suppliers
    ORDER BY CreatedAt);

DECLARE @altSupplierId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id
    FROM dbo.Suppliers
    WHERE Id <> @supplierId
    ORDER BY CreatedAt);

DECLARE @customerId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id
    FROM dbo.Customers
    ORDER BY CreatedAt);

DECLARE @altCustomerId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id
    FROM dbo.Customers
    WHERE Id <> @customerId
    ORDER BY CreatedAt);

DECLARE @variantId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id
    FROM dbo.LumberVariants
    ORDER BY CreatedAt);

IF @altSupplierId IS NULL SET @altSupplierId = @supplierId;
IF @altCustomerId IS NULL SET @altCustomerId = @customerId;

/* Shipment 1 - Complete flow from Ordered to DeliveredToDealer */
DECLARE @shipment1 UNIQUEIDENTIFIER = 'A9A0ED0F-3E1D-47D8-8C5D-25F1A436FF01';
IF NOT EXISTS (SELECT 1 FROM dbo.Shipments WHERE Id = @shipment1)
BEGIN
    DECLARE @s1Ordered DATETIME = DATEADD(DAY, -30, @now);
    DECLARE @s1Production DATETIME = DATEADD(DAY, -28, @now);
    DECLARE @s1Departed DATETIME = DATEADD(DAY, -26, @now);
    DECLARE @s1Train DATETIME = DATEADD(DAY, -25, @now);
    DECLARE @s1Vessel DATETIME = DATEADD(DAY, -21, @now);
    DECLARE @s1Port DATETIME = DATEADD(DAY, -15, @now);
    DECLARE @s1Customs DATETIME = DATEADD(DAY, -12, @now);
    DECLARE @s1Truck DATETIME = DATEADD(DAY, -8, @now);
    DECLARE @s1Warehouse DATETIME = DATEADD(DAY, -6, @now);
    DECLARE @s1Delivered DATETIME = DATEADD(DAY, -3, @now);

    INSERT INTO dbo.Shipments
        (Id, SupplierId, CustomerId, ReferenceNumber, ShipmentDate, EstimatedArrival, Status,
         LoadingPort, DischargePort, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    VALUES
        (@shipment1, @supplierId, @customerId, 'GMS-2025-001',
         @s1Ordered, DATEADD(DAY, 2, @s1Delivered), 9,
         'St. Petersburg', 'Mersin', 'Demo shipment covering full lifecycle.', @s1Ordered, NULL, @s1Delivered, NULL);

    DECLARE @stageTable TABLE(Status INT, StartedAt DATETIME, CompletedAt DATETIME, Notes NVARCHAR(500));

    INSERT INTO @stageTable VALUES
        (0, @s1Ordered, DATEADD(DAY, 1, @s1Ordered), N'Sipariş satışı onaylandı.'),
        (1, @s1Production, DATEADD(DAY, 1, @s1Production), N'Kereste üretimi başladı.'),
        (2, @s1Departed, DATEADD(DAY, 1, @s1Departed), N'Lojistik çıkışı planlandı.'),
        (3, @s1Train, DATEADD(DAY, 2, @s1Train), N'İç demiryolu transferi.'),
        (4, @s1Vessel, DATEADD(DAY, 4, @s1Vessel), N'Baltık çıkışlı gemide.'),
        (5, @s1Port, DATEADD(DAY, 2, @s1Port), N'İthalat limanında bekliyor.'),
        (6, @s1Customs, DATEADD(DAY, 2, @s1Customs), N'Gümrük işlemleri tamamlandı.'),
        (7, @s1Truck, DATEADD(DAY, 1, @s1Truck), N'Tır sevkiyatı planlandı.'),
        (8, @s1Warehouse, DATEADD(DAY, 2, @s1Warehouse), N'Depoya alındı, kalite kontrol yapıldı.'),
        (9, @s1Delivered, DATEADD(HOUR, 4, @s1Delivered), N'Bayi teslimatı tamamlandı.');

    INSERT INTO dbo.ShipmentStages
        (Id, ShipmentId, Status, StartedAt, CompletedAt, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    SELECT NEWID(), @shipment1, Status, StartedAt, CompletedAt, Notes, StartedAt, NULL, CompletedAt, NULL
    FROM @stageTable AS s
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ShipmentStages st
        WHERE st.ShipmentId = @shipment1 AND st.Status = s.Status);

    IF NOT EXISTS (SELECT 1 FROM dbo.CustomsProcesses WHERE ShipmentId = @shipment1)
    BEGIN
        INSERT INTO dbo.CustomsProcesses
            (Id, ShipmentId, Status, StartedAt, CompletedAt, DocumentNumber, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
        VALUES
            (NEWID(), @shipment1, 2, @s1Customs, DATEADD(DAY, 2, @s1Customs), 'GMS-CUS-2025-001',
             N'Dökümantasyon tamamlandı ve vergi ödemesi yapıldı.', @s1Customs, NULL, DATEADD(DAY, 2, @s1Customs), NULL);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.ShipmentTransportUnits WHERE ShipmentId = @shipment1)
    BEGIN
        INSERT INTO dbo.ShipmentTransportUnits (Id, ShipmentId, Mode, Identifier, Count)
        VALUES
            (NEWID(), @shipment1, 3, '40FT-HIGH-01', 6),
            (NEWID(), @shipment1, 4, 'TR-34-GMS-221', 3);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.ShipmentItems WHERE ShipmentId = @shipment1)
    BEGIN
        INSERT INTO dbo.ShipmentItems (Id, ShipmentId, VariantId, Quantity, Volume)
        VALUES
            (NEWID(), @shipment1, @variantId, 120, 68.4);
    END;
END;

/* Shipment 2 - Currently in customs */
DECLARE @shipment2 UNIQUEIDENTIFIER = 'B7B75C0A-8826-4B5A-9F3B-7E9CE3EAF102';
IF NOT EXISTS (SELECT 1 FROM dbo.Shipments WHERE Id = @shipment2)
BEGIN
    DECLARE @s2Ordered DATETIME = DATEADD(DAY, -18, @now);
    DECLARE @s2Production DATETIME = DATEADD(DAY, -17, @now);
    DECLARE @s2Departed DATETIME = DATEADD(DAY, -15, @now);
    DECLARE @s2Train DATETIME = DATEADD(DAY, -14, @now);
    DECLARE @s2Vessel DATETIME = DATEADD(DAY, -10, @now);
    DECLARE @s2Port DATETIME = DATEADD(DAY, -6, @now);
    DECLARE @s2CustomsStart DATETIME = DATEADD(DAY, -4, @now);

    INSERT INTO dbo.Shipments
        (Id, SupplierId, CustomerId, ReferenceNumber, ShipmentDate, EstimatedArrival, Status,
         LoadingPort, DischargePort, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    VALUES
        (@shipment2, @altSupplierId, @altCustomerId, 'GMS-2025-009',
         @s2Ordered, DATEADD(DAY, 7, @now), 6,
         'Arkhangelsk', 'Samsun', 'Sevkiyat gümrükte takılı kaldı.', @s2Ordered, NULL, @now, NULL);

    INSERT INTO dbo.ShipmentStages
        (Id, ShipmentId, Status, StartedAt, CompletedAt, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    SELECT NEWID(), @shipment2, s.Status, s.StartedAt, s.CompletedAt, s.Notes, s.StartedAt, NULL, s.CompletedAt, NULL
    FROM (VALUES
            (0, @s2Ordered, DATEADD(HOUR, 12, @s2Ordered), N'Satın alma onaylandı.'),
            (1, @s2Production, DATEADD(DAY, 1, @s2Production), N'Üretim planı tamamlandı.'),
            (2, @s2Departed, DATEADD(DAY, 1, @s2Departed), N'Liman transferi için yola çıktı.'),
            (3, @s2Train, DATEADD(DAY, 3, @s2Train), N'Rusya iç transferi.'),
            (4, @s2Vessel, DATEADD(DAY, 3, @s2Vessel), N'Karadeniz çıkışlı gemide.'),
            (5, @s2Port, DATEADD(DAY, 1, @s2Port), N'Limanda boşaltma sırada.'),
            (6, @s2CustomsStart, NULL, N'Gümrük beyannamesi bekliyor.')
        ) AS s(Status, StartedAt, CompletedAt, Notes)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ShipmentStages st
        WHERE st.ShipmentId = @shipment2 AND st.Status = s.Status);

    IF NOT EXISTS (SELECT 1 FROM dbo.CustomsProcesses WHERE ShipmentId = @shipment2)
    BEGIN
        INSERT INTO dbo.CustomsProcesses
            (Id, ShipmentId, Status, StartedAt, CompletedAt, DocumentNumber, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
        VALUES
            (NEWID(), @shipment2, 1, @s2CustomsStart, NULL, 'GMS-CUS-2025-009',
             N'Eksik evrak nedeniyle incelemede.', @s2CustomsStart, NULL, NULL, NULL);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.ShipmentTransportUnits WHERE ShipmentId = @shipment2)
    BEGIN
        INSERT INTO dbo.ShipmentTransportUnits (Id, ShipmentId, Mode, Identifier, Count)
        VALUES
            (NEWID(), @shipment2, 2, 'MV-BLACKSEA-77', 1),
            (NEWID(), @shipment2, 1, 'RUS-RAIL-552', 12);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.ShipmentItems WHERE ShipmentId = @shipment2)
    BEGIN
        INSERT INTO dbo.ShipmentItems (Id, ShipmentId, VariantId, Quantity, Volume)
        VALUES
            (NEWID(), @shipment2, @variantId, 80, 42.6);
    END;
END;

/* Shipment 3 - Early stage (ProductionStarted) */
DECLARE @shipment3 UNIQUEIDENTIFIER = 'D4656EDC-8D0F-4FC8-9A11-3B4B6E21D203';
IF NOT EXISTS (SELECT 1 FROM dbo.Shipments WHERE Id = @shipment3)
BEGIN
    DECLARE @s3Ordered DATETIME = DATEADD(DAY, -5, @now);
    DECLARE @s3Production DATETIME = DATEADD(DAY, -3, @now);

    INSERT INTO dbo.Shipments
        (Id, SupplierId, CustomerId, ReferenceNumber, ShipmentDate, EstimatedArrival, Status,
         LoadingPort, DischargePort, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    VALUES
        (@shipment3, @supplierId, NULL, 'GMS-2025-015',
         @s3Ordered, DATEADD(DAY, 25, @now), 1,
         'Murmansk', 'İzmir', 'Üretim sürecinde olan sevkiyat.', @s3Ordered, NULL, @s3Production, NULL);

    INSERT INTO dbo.ShipmentStages
        (Id, ShipmentId, Status, StartedAt, CompletedAt, Notes, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    SELECT NEWID(), @shipment3, s.Status, s.StartedAt, s.CompletedAt, s.Notes, s.StartedAt, NULL, s.CompletedAt, NULL
    FROM (VALUES
            (0, @s3Ordered, DATEADD(DAY, 1, @s3Ordered), N'Satın alma siparişi oluşturuldu.'),
            (1, @s3Production, NULL, N'Üretim hattına alındı.')
        ) AS s(Status, StartedAt, CompletedAt, Notes)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ShipmentStages st
        WHERE st.ShipmentId = @shipment3 AND st.Status = s.Status);
END;

COMMIT TRANSACTION;


