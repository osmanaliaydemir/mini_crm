using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailShipmentUpdates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailPaymentReminders = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailWarehouseAlerts = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailCustomerInteractions = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailSystemAnnouncements = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InAppShipmentUpdates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InAppPaymentReminders = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InAppWarehouseAlerts = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InAppCustomerInteractions = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InAppSystemAnnouncements = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationPreferences");
        }
    }
}
