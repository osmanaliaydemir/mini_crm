BEGIN TRANSACTION;
ALTER TABLE [EmailAutomationRuleRecipients] ADD [RoleName] nvarchar(128) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251117142036_AddEmailAutomationRoleRecipients', N'9.0.10');

COMMIT;
GO

