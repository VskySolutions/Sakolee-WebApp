BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    ALTER TABLE [Users] ADD [IsProtected] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    ALTER TABLE [Tenants] ADD [AddressId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    ALTER TABLE [Tenants] ADD [PersonId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    CREATE INDEX [IX_Tenants_AddressId] ON [Tenants] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    CREATE INDEX [IX_Tenants_PersonId] ON [Tenants] ([PersonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    ALTER TABLE [Tenants] ADD CONSTRAINT [FK_Tenants_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    ALTER TABLE [Tenants] ADD CONSTRAINT [FK_Tenants_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915121731_AddTenantAdminAndAddress'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915121731_AddTenantAdminAndAddress', N'9.0.0');
END;

COMMIT;
GO

