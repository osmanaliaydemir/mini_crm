using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionConcurrencyControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tüm tablolar için RowVersion ekleme - eğer zaten varsa ekleme
            var tables = new[]
            {
                "WarehouseUnloadings",
                "Warehouses",
                "Suppliers",
                "ShipmentStages",
                "Shipments",
                "PaymentPlans",
                "PaymentInstallments",
                "NotificationPreferences",
                "LumberVariants",
                "CustomsProcesses",
                "Customers",
                "CustomerInteractions",
                "CashTransactions"
            };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('{table}') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [{table}] ADD [RowVersion] rowversion NOT NULL;
                    END
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tüm tablolar için RowVersion silme - eğer varsa sil
            var tables = new[]
            {
                "WarehouseUnloadings",
                "Warehouses",
                "Suppliers",
                "ShipmentStages",
                "Shipments",
                "PaymentPlans",
                "PaymentInstallments",
                "NotificationPreferences",
                "LumberVariants",
                "CustomsProcesses",
                "Customers",
                "CustomerInteractions",
                "CashTransactions"
            };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('{table}') AND name = 'RowVersion')
                    BEGIN
                        ALTER TABLE [{table}] DROP COLUMN [RowVersion];
                    END
                ");
            }
        }
    }
}
