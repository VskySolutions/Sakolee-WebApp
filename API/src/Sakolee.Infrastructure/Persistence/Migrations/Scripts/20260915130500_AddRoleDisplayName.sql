BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915130500_AddRoleDisplayName'
)
BEGIN
    ALTER TABLE [Roles] ADD [DisplayName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915130500_AddRoleDisplayName'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915130500_AddRoleDisplayName', N'9.0.0');
END;

COMMIT;
GO

