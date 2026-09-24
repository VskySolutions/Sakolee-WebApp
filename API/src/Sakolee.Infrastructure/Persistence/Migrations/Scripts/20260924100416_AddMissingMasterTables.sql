BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924100416_AddMissingMasterTables'
)
BEGIN

    IF OBJECT_ID(N'[BillingMethod]', N'U') IS NULL
    BEGIN
        CREATE TABLE [BillingMethod] (
            [Id] uniqueidentifier NOT NULL,
            [TenantId] uniqueidentifier NULL,
            [Name] nvarchar(max) NOT NULL,
            [Active] bit NOT NULL,
            [CreatedById] uniqueidentifier NULL,
            [CreatedOnUtc] datetime2 NOT NULL,
            [UpdatedById] uniqueidentifier NULL,
            [UpdatedOnUtc] datetime2 NOT NULL,
            [Deleted] bit NOT NULL,
            [DeletedOnUtc] datetime2 NULL,
            CONSTRAINT [PK_BillingMethod] PRIMARY KEY ([Id])
        );
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924100416_AddMissingMasterTables'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_BillingMethod_TenantId' AND [object_id] = OBJECT_ID(N'[BillingMethod]'))
        CREATE INDEX [IX_BillingMethod_TenantId] ON [BillingMethod] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924100416_AddMissingMasterTables'
)
BEGIN

    IF OBJECT_ID(N'[FK_BillingMethod_Tenants_TenantId]', N'F') IS NULL
        ALTER TABLE [BillingMethod] ADD CONSTRAINT [FK_BillingMethod_Tenants_TenantId]
            FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924100416_AddMissingMasterTables'
)
BEGIN

    IF OBJECT_ID(N'[TShirtSize]', N'U') IS NULL
    BEGIN
        CREATE TABLE [TShirtSize] (
            [Id] nvarchar(450) NOT NULL,
            [TenentId] nvarchar(450) NOT NULL,
            [Name] nvarchar(50) NOT NULL,
            [CreatedOnUtc] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
            [CreatedById] nvarchar(450) NULL,
            [UpdatedOnUtc] datetime2 NULL,
            [UpdatedById] nvarchar(450) NULL,
            [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
            CONSTRAINT [PK_TShirtSize] PRIMARY KEY ([Id])
        );
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924100416_AddMissingMasterTables'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_TShirtSize_TenentId_Name' AND [object_id] = OBJECT_ID(N'[TShirtSize]'))
        EXEC(N'CREATE UNIQUE INDEX [IX_TShirtSize_TenentId_Name] ON [TShirtSize] ([TenentId], [Name]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924100416_AddMissingMasterTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924100416_AddMissingMasterTables', N'9.0.15');
END;

COMMIT;
GO

