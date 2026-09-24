BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924093210_AddBillingCycle'
)
BEGIN
    CREATE TABLE [BillingCycle] (
        [Id] nvarchar(450) NOT NULL,
        [TenentId] nvarchar(450) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [CreatedOnUtc] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
        [CreatedById] nvarchar(450) NULL,
        [UpdatedOnUtc] datetime2 NULL,
        [UpdatedById] nvarchar(450) NULL,
        [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_BillingCycle] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924093210_AddBillingCycle'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_BillingCycle_TenentId_Name] ON [BillingCycle] ([TenentId], [Name]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924093210_AddBillingCycle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924093210_AddBillingCycle', N'9.0.15');
END;

COMMIT;
GO

