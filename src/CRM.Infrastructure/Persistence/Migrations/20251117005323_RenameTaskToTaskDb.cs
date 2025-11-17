using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTaskToTaskDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if Tasks table exists and rename it, otherwise check if TaskDb exists, otherwise create new TaskDb table
            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaskDb]') AND type in (N'U'))
                BEGIN
                    -- Rename back to Tasks if reverting
                    EXEC sp_rename 'TaskDb', 'Tasks';
                    
                    -- Rename indexes back
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_AssignedToUserId' AND object_id = OBJECT_ID('dbo.Tasks'))
                        EXEC sp_rename 'Tasks.IX_TaskDb_AssignedToUserId', 'IX_Tasks_AssignedToUserId', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_Status' AND object_id = OBJECT_ID('dbo.Tasks'))
                        EXEC sp_rename 'Tasks.IX_TaskDb_Status', 'IX_Tasks_Status', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_DueDate' AND object_id = OBJECT_ID('dbo.Tasks'))
                        EXEC sp_rename 'Tasks.IX_TaskDb_DueDate', 'IX_Tasks_DueDate', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_CreatedAt' AND object_id = OBJECT_ID('dbo.Tasks'))
                        EXEC sp_rename 'Tasks.IX_TaskDb_CreatedAt', 'IX_Tasks_CreatedAt', 'INDEX';
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskDb_RelatedEntity' AND object_id = OBJECT_ID('dbo.Tasks'))
                        EXEC sp_rename 'Tasks.IX_TaskDb_RelatedEntity', 'IX_Tasks_RelatedEntity', 'INDEX';
                    
                    -- Rename primary key back
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_TaskDb]') AND type in (N'PK'))
                        EXEC sp_rename 'PK_TaskDb', 'PK_Tasks', 'OBJECT';
                    
                    -- Rename foreign key constraint back
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TaskDb_AspNetUsers_AssignedToUserId')
                        EXEC sp_rename 'FK_TaskDb_AspNetUsers_AssignedToUserId', 'FK_Tasks_AspNetUsers_AssignedToUserId', 'OBJECT';
                END
            ");
        }
    }
}
