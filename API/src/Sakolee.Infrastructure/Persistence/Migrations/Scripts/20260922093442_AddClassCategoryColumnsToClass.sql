BEGIN TRANSACTION;
IF COL_LENGTH(N'[Class]', N'Category1Id') IS NULL
BEGIN
    ALTER TABLE [Class] ADD [Category1Id] nvarchar(450) NULL;
END;

IF COL_LENGTH(N'[Class]', N'Category2Id') IS NULL
BEGIN
    ALTER TABLE [Class] ADD [Category2Id] nvarchar(450) NULL;
END;

IF COL_LENGTH(N'[Class]', N'Category3Id') IS NULL
BEGIN
    ALTER TABLE [Class] ADD [Category3Id] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922093442_AddClassCategoryColumnsToClass'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922093442_AddClassCategoryColumnsToClass', N'9.0.0');
END;

COMMIT;
GO
