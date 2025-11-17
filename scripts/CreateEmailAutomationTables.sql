-- Email Automation Tables Creation Script
-- This script creates the EmailAutomationRules and EmailAutomationRuleRecipients tables

BEGIN TRANSACTION;

-- Create EmailAutomationRules table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailAutomationRules]') AND type in (N'U'))
BEGIN
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
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_EmailAutomationRules] PRIMARY KEY ([Id])
    );
    PRINT 'EmailAutomationRules table created successfully.';
END
ELSE
BEGIN
    PRINT 'EmailAutomationRules table already exists.';
END
GO

-- Create EmailAutomationRuleRecipients table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailAutomationRuleRecipients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [EmailAutomationRuleRecipients] (
        [Id] uniqueidentifier NOT NULL,
        [RuleId] uniqueidentifier NOT NULL,
        [RecipientType] int NOT NULL,
        [UserId] uniqueidentifier NULL,
        [EmailAddress] nvarchar(256) NULL,
        [RoleName] nvarchar(128) NULL,
        CONSTRAINT [PK_EmailAutomationRuleRecipients] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailAutomationRuleRecipients_EmailAutomationRules_RuleId] 
            FOREIGN KEY ([RuleId]) REFERENCES [EmailAutomationRules] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_EmailAutomationRuleRecipients_RuleId] 
        ON [EmailAutomationRuleRecipients] ([RuleId]);
    
    PRINT 'EmailAutomationRuleRecipients table created successfully.';
END
ELSE
BEGIN
    -- Check if RoleName column exists, if not add it
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EmailAutomationRuleRecipients]') AND name = 'RoleName')
    BEGIN
        ALTER TABLE [EmailAutomationRuleRecipients] ADD [RoleName] nvarchar(128) NULL;
        PRINT 'RoleName column added to EmailAutomationRuleRecipients table.';
    END
    ELSE
    BEGIN
        PRINT 'EmailAutomationRuleRecipients table already exists with RoleName column.';
    END
END
GO

-- Insert migration history records if they don't exist
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251117120723_AddEmailAutomationRules')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251117120723_AddEmailAutomationRules', N'9.0.10');
    PRINT 'Migration history record added for AddEmailAutomationRules.';
END
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251117142036_AddEmailAutomationRoleRecipients')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251117142036_AddEmailAutomationRoleRecipients', N'9.0.10');
    PRINT 'Migration history record added for AddEmailAutomationRoleRecipients.';
END
GO

COMMIT;
PRINT 'Email Automation tables creation completed successfully.';
GO

