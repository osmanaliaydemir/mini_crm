using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create indexes with IF NOT EXISTS check for SQL Server
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WarehouseUnloadings_UnloadedAt' AND object_id = OBJECT_ID('dbo.WarehouseUnloadings'))
                    CREATE NONCLUSTERED INDEX [IX_WarehouseUnloadings_UnloadedAt] ON [dbo].[WarehouseUnloadings] ([UnloadedAt]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WarehouseUnloadings_WarehouseId_UnloadedAt' AND object_id = OBJECT_ID('dbo.WarehouseUnloadings'))
                    CREATE NONCLUSTERED INDEX [IX_WarehouseUnloadings_WarehouseId_UnloadedAt] ON [dbo].[WarehouseUnloadings] ([WarehouseId], [UnloadedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Warehouses_CreatedAt' AND object_id = OBJECT_ID('dbo.Warehouses'))
                    CREATE NONCLUSTERED INDEX [IX_Warehouses_CreatedAt] ON [dbo].[Warehouses] ([CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Warehouses_Name' AND object_id = OBJECT_ID('dbo.Warehouses'))
                    CREATE NONCLUSTERED INDEX [IX_Warehouses_Name] ON [dbo].[Warehouses] ([Name]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Country' AND object_id = OBJECT_ID('dbo.Suppliers'))
                    CREATE NONCLUSTERED INDEX [IX_Suppliers_Country] ON [dbo].[Suppliers] ([Country]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_CreatedAt' AND object_id = OBJECT_ID('dbo.Suppliers'))
                    CREATE NONCLUSTERED INDEX [IX_Suppliers_CreatedAt] ON [dbo].[Suppliers] ([CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Name' AND object_id = OBJECT_ID('dbo.Suppliers'))
                    CREATE NONCLUSTERED INDEX [IX_Suppliers_Name] ON [dbo].[Suppliers] ([Name]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_CreatedAt' AND object_id = OBJECT_ID('dbo.Shipments'))
                    CREATE NONCLUSTERED INDEX [IX_Shipments_CreatedAt] ON [dbo].[Shipments] ([CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_ReferenceNumber' AND object_id = OBJECT_ID('dbo.Shipments'))
                    CREATE UNIQUE NONCLUSTERED INDEX [IX_Shipments_ReferenceNumber] ON [dbo].[Shipments] ([ReferenceNumber]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_Status' AND object_id = OBJECT_ID('dbo.Shipments'))
                    CREATE NONCLUSTERED INDEX [IX_Shipments_Status] ON [dbo].[Shipments] ([Status]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_Status_CreatedAt' AND object_id = OBJECT_ID('dbo.Shipments'))
                    CREATE NONCLUSTERED INDEX [IX_Shipments_Status_CreatedAt] ON [dbo].[Shipments] ([Status], [CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_CustomerId' AND object_id = OBJECT_ID('dbo.Shipments'))
                    CREATE NONCLUSTERED INDEX [IX_Shipments_CustomerId] ON [dbo].[Shipments] ([CustomerId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_CreatedAt' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    CREATE NONCLUSTERED INDEX [IX_PaymentPlans_CreatedAt] ON [dbo].[PaymentPlans] ([CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_CustomerId' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    CREATE NONCLUSTERED INDEX [IX_PaymentPlans_CustomerId] ON [dbo].[PaymentPlans] ([CustomerId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_CustomerId_CreatedAt' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    CREATE NONCLUSTERED INDEX [IX_PaymentPlans_CustomerId_CreatedAt] ON [dbo].[PaymentPlans] ([CustomerId], [CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_ShipmentId' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    CREATE NONCLUSTERED INDEX [IX_PaymentPlans_ShipmentId] ON [dbo].[PaymentPlans] ([ShipmentId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentInstallments_DueDate' AND object_id = OBJECT_ID('dbo.PaymentInstallments'))
                    CREATE NONCLUSTERED INDEX [IX_PaymentInstallments_DueDate] ON [dbo].[PaymentInstallments] ([DueDate]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentInstallments_PaymentPlanId' AND object_id = OBJECT_ID('dbo.PaymentInstallments'))
                    CREATE NONCLUSTERED INDEX [IX_PaymentInstallments_PaymentPlanId] ON [dbo].[PaymentInstallments] ([PaymentPlanId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_CreatedAt' AND object_id = OBJECT_ID('dbo.Customers'))
                    CREATE NONCLUSTERED INDEX [IX_Customers_CreatedAt] ON [dbo].[Customers] ([CreatedAt]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Email' AND object_id = OBJECT_ID('dbo.Customers'))
                    CREATE NONCLUSTERED INDEX [IX_Customers_Email] ON [dbo].[Customers] ([Email]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Name' AND object_id = OBJECT_ID('dbo.Customers'))
                    CREATE NONCLUSTERED INDEX [IX_Customers_Name] ON [dbo].[Customers] ([Name]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Name_Segment' AND object_id = OBJECT_ID('dbo.Customers'))
                    CREATE NONCLUSTERED INDEX [IX_Customers_Name_Segment] ON [dbo].[Customers] ([Name], [Segment]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Segment' AND object_id = OBJECT_ID('dbo.Customers'))
                    CREATE NONCLUSTERED INDEX [IX_Customers_Segment] ON [dbo].[Customers] ([Segment]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_CustomerId_InteractionDate' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    CREATE NONCLUSTERED INDEX [IX_CustomerInteractions_CustomerId_InteractionDate] ON [dbo].[CustomerInteractions] ([CustomerId], [InteractionDate]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_InteractionDate' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    CREATE NONCLUSTERED INDEX [IX_CustomerInteractions_InteractionDate] ON [dbo].[CustomerInteractions] ([InteractionDate]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_InteractionType' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    CREATE NONCLUSTERED INDEX [IX_CustomerInteractions_InteractionType] ON [dbo].[CustomerInteractions] ([InteractionType]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_CustomerId' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    CREATE NONCLUSTERED INDEX [IX_CustomerInteractions_CustomerId] ON [dbo].[CustomerInteractions] ([CustomerId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerContacts_Email' AND object_id = OBJECT_ID('dbo.CustomerContacts'))
                    CREATE NONCLUSTERED INDEX [IX_CustomerContacts_Email] ON [dbo].[CustomerContacts] ([Email]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerContacts_CustomerId' AND object_id = OBJECT_ID('dbo.CustomerContacts'))
                    CREATE NONCLUSTERED INDEX [IX_CustomerContacts_CustomerId] ON [dbo].[CustomerContacts] ([CustomerId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_RelatedCustomerId' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    CREATE NONCLUSTERED INDEX [IX_CashTransactions_RelatedCustomerId] ON [dbo].[CashTransactions] ([RelatedCustomerId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_RelatedShipmentId' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    CREATE NONCLUSTERED INDEX [IX_CashTransactions_RelatedShipmentId] ON [dbo].[CashTransactions] ([RelatedShipmentId]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_TransactionDate' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    CREATE NONCLUSTERED INDEX [IX_CashTransactions_TransactionDate] ON [dbo].[CashTransactions] ([TransactionDate]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_TransactionDate_TransactionType' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    CREATE NONCLUSTERED INDEX [IX_CashTransactions_TransactionDate_TransactionType] ON [dbo].[CashTransactions] ([TransactionDate], [TransactionType]);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_TransactionType' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    CREATE NONCLUSTERED INDEX [IX_CashTransactions_TransactionType] ON [dbo].[CashTransactions] ([TransactionType]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes with IF EXISTS check
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WarehouseUnloadings_UnloadedAt' AND object_id = OBJECT_ID('dbo.WarehouseUnloadings'))
                    DROP INDEX [IX_WarehouseUnloadings_UnloadedAt] ON [dbo].[WarehouseUnloadings];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WarehouseUnloadings_WarehouseId_UnloadedAt' AND object_id = OBJECT_ID('dbo.WarehouseUnloadings'))
                    DROP INDEX [IX_WarehouseUnloadings_WarehouseId_UnloadedAt] ON [dbo].[WarehouseUnloadings];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Warehouses_CreatedAt' AND object_id = OBJECT_ID('dbo.Warehouses'))
                    DROP INDEX [IX_Warehouses_CreatedAt] ON [dbo].[Warehouses];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Warehouses_Name' AND object_id = OBJECT_ID('dbo.Warehouses'))
                    DROP INDEX [IX_Warehouses_Name] ON [dbo].[Warehouses];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Country' AND object_id = OBJECT_ID('dbo.Suppliers'))
                    DROP INDEX [IX_Suppliers_Country] ON [dbo].[Suppliers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_CreatedAt' AND object_id = OBJECT_ID('dbo.Suppliers'))
                    DROP INDEX [IX_Suppliers_CreatedAt] ON [dbo].[Suppliers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Name' AND object_id = OBJECT_ID('dbo.Suppliers'))
                    DROP INDEX [IX_Suppliers_Name] ON [dbo].[Suppliers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_CreatedAt' AND object_id = OBJECT_ID('dbo.Shipments'))
                    DROP INDEX [IX_Shipments_CreatedAt] ON [dbo].[Shipments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_ReferenceNumber' AND object_id = OBJECT_ID('dbo.Shipments'))
                    DROP INDEX [IX_Shipments_ReferenceNumber] ON [dbo].[Shipments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_Status' AND object_id = OBJECT_ID('dbo.Shipments'))
                    DROP INDEX [IX_Shipments_Status] ON [dbo].[Shipments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_Status_CreatedAt' AND object_id = OBJECT_ID('dbo.Shipments'))
                    DROP INDEX [IX_Shipments_Status_CreatedAt] ON [dbo].[Shipments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_CustomerId' AND object_id = OBJECT_ID('dbo.Shipments'))
                    DROP INDEX [IX_Shipments_CustomerId] ON [dbo].[Shipments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_CreatedAt' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    DROP INDEX [IX_PaymentPlans_CreatedAt] ON [dbo].[PaymentPlans];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_CustomerId' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    DROP INDEX [IX_PaymentPlans_CustomerId] ON [dbo].[PaymentPlans];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_CustomerId_CreatedAt' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    DROP INDEX [IX_PaymentPlans_CustomerId_CreatedAt] ON [dbo].[PaymentPlans];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentPlans_ShipmentId' AND object_id = OBJECT_ID('dbo.PaymentPlans'))
                    DROP INDEX [IX_PaymentPlans_ShipmentId] ON [dbo].[PaymentPlans];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentInstallments_DueDate' AND object_id = OBJECT_ID('dbo.PaymentInstallments'))
                    DROP INDEX [IX_PaymentInstallments_DueDate] ON [dbo].[PaymentInstallments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PaymentInstallments_PaymentPlanId' AND object_id = OBJECT_ID('dbo.PaymentInstallments'))
                    DROP INDEX [IX_PaymentInstallments_PaymentPlanId] ON [dbo].[PaymentInstallments];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_CreatedAt' AND object_id = OBJECT_ID('dbo.Customers'))
                    DROP INDEX [IX_Customers_CreatedAt] ON [dbo].[Customers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Email' AND object_id = OBJECT_ID('dbo.Customers'))
                    DROP INDEX [IX_Customers_Email] ON [dbo].[Customers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Name' AND object_id = OBJECT_ID('dbo.Customers'))
                    DROP INDEX [IX_Customers_Name] ON [dbo].[Customers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Name_Segment' AND object_id = OBJECT_ID('dbo.Customers'))
                    DROP INDEX [IX_Customers_Name_Segment] ON [dbo].[Customers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Segment' AND object_id = OBJECT_ID('dbo.Customers'))
                    DROP INDEX [IX_Customers_Segment] ON [dbo].[Customers];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_CustomerId_InteractionDate' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    DROP INDEX [IX_CustomerInteractions_CustomerId_InteractionDate] ON [dbo].[CustomerInteractions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_InteractionDate' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    DROP INDEX [IX_CustomerInteractions_InteractionDate] ON [dbo].[CustomerInteractions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_InteractionType' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    DROP INDEX [IX_CustomerInteractions_InteractionType] ON [dbo].[CustomerInteractions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerInteractions_CustomerId' AND object_id = OBJECT_ID('dbo.CustomerInteractions'))
                    DROP INDEX [IX_CustomerInteractions_CustomerId] ON [dbo].[CustomerInteractions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerContacts_Email' AND object_id = OBJECT_ID('dbo.CustomerContacts'))
                    DROP INDEX [IX_CustomerContacts_Email] ON [dbo].[CustomerContacts];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerContacts_CustomerId' AND object_id = OBJECT_ID('dbo.CustomerContacts'))
                    DROP INDEX [IX_CustomerContacts_CustomerId] ON [dbo].[CustomerContacts];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_RelatedCustomerId' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    DROP INDEX [IX_CashTransactions_RelatedCustomerId] ON [dbo].[CashTransactions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_RelatedShipmentId' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    DROP INDEX [IX_CashTransactions_RelatedShipmentId] ON [dbo].[CashTransactions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_TransactionDate' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    DROP INDEX [IX_CashTransactions_TransactionDate] ON [dbo].[CashTransactions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_TransactionDate_TransactionType' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    DROP INDEX [IX_CashTransactions_TransactionDate_TransactionType] ON [dbo].[CashTransactions];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CashTransactions_TransactionType' AND object_id = OBJECT_ID('dbo.CashTransactions'))
                    DROP INDEX [IX_CashTransactions_TransactionType] ON [dbo].[CashTransactions];
            ");
        }
    }
}
