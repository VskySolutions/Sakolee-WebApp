BEGIN TRANSACTION;
IF COL_LENGTH(N'[Students]', N'AllergiesNotes') IS NULL
BEGIN
    ALTER TABLE [Students] ADD [AllergiesNotes] nvarchar(500) NULL;
END;

IF COL_LENGTH(N'[Students]', N'AllowTextMessaging') IS NULL
BEGIN
    ALTER TABLE [Students] ADD [AllowTextMessaging] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF COL_LENGTH(N'[Students]', N'DisabilitiesNotes') IS NULL
BEGIN
    ALTER TABLE [Students] ADD [DisabilitiesNotes] nvarchar(500) NULL;
END;

IF COL_LENGTH(N'[Students]', N'HasImmunizations') IS NULL
BEGIN
    ALTER TABLE [Students] ADD [HasImmunizations] nvarchar(20) NULL;
END;

IF COL_LENGTH(N'[Students]', N'HealthInsuranceCarrier') IS NULL
BEGIN
    ALTER TABLE [Students] ADD [HealthInsuranceCarrier] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922100530_AddStudentColumnsForPrototypeForm'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922100530_AddStudentColumnsForPrototypeForm', N'9.0.0');
END;

COMMIT;
GO
