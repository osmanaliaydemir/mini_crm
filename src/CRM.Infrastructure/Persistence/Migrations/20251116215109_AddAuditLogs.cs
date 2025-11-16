using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tablo zaten varsa oluşturma işlemini atla
            migrationBuilder.Sql(@"
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
            ");

            // İndeksler
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Action' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Action] ON [AuditLogs]([Action] ASC);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityId] ON [AuditLogs]([EntityId] ASC);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType] ON [AuditLogs]([EntityType] ASC);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType_EntityId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs]([EntityType] ASC, [EntityId] ASC);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Timestamp' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs]([Timestamp] DESC);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserId' AND object_id = OBJECT_ID('dbo.AuditLogs'))
                    CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] ON [AuditLogs]([UserId] ASC);
            ");

            // NOT: IX_AuditLogs_UserId_Timestamp composite index key length sınırı (900 byte) aştığı için oluşturulmuyor
            // UserId NVARCHAR(450) = 900 byte + Timestamp = 908 byte > 900 byte limit
            // Bu index gerekli değil çünkü UserId ve Timestamp ayrı ayrı index'lenmiş
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
