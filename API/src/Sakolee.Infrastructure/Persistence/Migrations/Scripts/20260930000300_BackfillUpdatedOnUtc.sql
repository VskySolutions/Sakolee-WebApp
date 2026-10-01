-- AuditableEntity.UpdatedOnUtc is a non-nullable DateTime, but ClassCategory and AccountType were created
-- with UpdatedOnUtc datetime2 NULL. Rows never updated have NULL there, so any read throws
-- SqlNullValueException ("Data is Null"). Backfill from CreatedOnUtc and make the column NOT NULL with a
-- default so it cannot recur.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

UPDATE [ClassCategory] SET [UpdatedOnUtc] = [CreatedOnUtc] WHERE [UpdatedOnUtc] IS NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClassCategory') AND name = 'UpdatedOnUtc' AND is_nullable = 1)
    ALTER TABLE [ClassCategory] ALTER COLUMN [UpdatedOnUtc] datetime2(6) NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClassCategory')
               AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('ClassCategory'), 'UpdatedOnUtc', 'ColumnId'))
    ALTER TABLE [ClassCategory] ADD CONSTRAINT [DF_ClassCategory_UpdatedOnUtc] DEFAULT (SYSUTCDATETIME()) FOR [UpdatedOnUtc];
GO

UPDATE [AccountType] SET [UpdatedOnUtc] = [CreatedOnUtc] WHERE [UpdatedOnUtc] IS NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AccountType') AND name = 'UpdatedOnUtc' AND is_nullable = 1)
    ALTER TABLE [AccountType] ALTER COLUMN [UpdatedOnUtc] datetime2(6) NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('AccountType')
               AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('AccountType'), 'UpdatedOnUtc', 'ColumnId'))
    ALTER TABLE [AccountType] ADD CONSTRAINT [DF_AccountType_UpdatedOnUtc] DEFAULT (SYSUTCDATETIME()) FOR [UpdatedOnUtc];
GO
