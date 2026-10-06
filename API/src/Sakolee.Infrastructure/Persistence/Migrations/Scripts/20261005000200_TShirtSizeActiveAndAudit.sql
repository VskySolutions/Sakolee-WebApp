-- Brings TShirtSize in line with TShirtSize/TShirtSizeConfiguration:
--   * Active (bit NOT NULL, default 1) — mapped by the entity but never added by any migration or script;
--     without it every T-Shirt Size query fails with "Invalid column name 'Active'".
--   * DeletedOnUtc (datetime2 NULL) and UpdatedOnUtc NOT NULL — from the AddTShirtSizeAuditableEntity
--     EF migration, which databases kept in sync through these scripts may not have had applied.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('TShirtSize', 'Active') IS NULL
BEGIN
    -- Existing sizes stay usable: the default fills every current row with 1 (active).
    ALTER TABLE [TShirtSize] ADD [Active] bit NOT NULL
        CONSTRAINT [DF_TShirtSize_Active] DEFAULT CAST(1 AS bit);
END;
GO

IF COL_LENGTH('TShirtSize', 'DeletedOnUtc') IS NULL
BEGIN
    ALTER TABLE [TShirtSize] ADD [DeletedOnUtc] datetime2 NULL;
END;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TShirtSize') AND name = 'UpdatedOnUtc' AND is_nullable = 1
)
BEGIN
    UPDATE [TShirtSize] SET [UpdatedOnUtc] = [CreatedOnUtc] WHERE [UpdatedOnUtc] IS NULL;
    ALTER TABLE [TShirtSize] ALTER COLUMN [UpdatedOnUtc] datetime2 NOT NULL;
END;
GO
