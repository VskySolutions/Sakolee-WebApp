BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917082807_AddClassCategory'
)
BEGIN
    CREATE TABLE [ClassCategory] (
        [Id] nvarchar(450) NOT NULL,
        [TenentId] nvarchar(450) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CategoryType] nvarchar(30) NULL,
        [CreatedOnUtc] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
        [CreatedById] nvarchar(450) NULL,
        [UpdatedOnUtc] datetime2 NULL,
        [UpdatedById] nvarchar(450) NULL,
        [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ClassCategory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917082807_AddClassCategory'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ClassCategory_TenentId_CategoryType_Name] ON [ClassCategory] ([TenentId], [CategoryType], [Name]) WHERE [Deleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917082807_AddClassCategory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917082807_AddClassCategory', N'9.0.0');
END;

COMMIT;
GO

