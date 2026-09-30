-- Moves ClassSessions onto the standard AuditableEntity columns (ClassSessions : AuditableEntity):
--   CreatedById / CreatedOnUtc / UpdatedById / UpdatedOnUtc / Deleted / DeletedOnUtc (Active already exists).
-- Backfills them from the legacy CreatedBy (nvarchar user id) / CreatedOn / UpdatedBy / UpdatedOn /
-- IsDeleted columns, then drops the legacy columns (EF no longer maps them).
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('ClassSessions', 'CreatedById') IS NULL
    ALTER TABLE [ClassSessions] ADD [CreatedById] uniqueidentifier NULL;
IF COL_LENGTH('ClassSessions', 'CreatedOnUtc') IS NULL
    ALTER TABLE [ClassSessions] ADD [CreatedOnUtc] datetime2 NOT NULL CONSTRAINT [DF_ClassSessions_CreatedOnUtc] DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('ClassSessions', 'UpdatedById') IS NULL
    ALTER TABLE [ClassSessions] ADD [UpdatedById] uniqueidentifier NULL;
IF COL_LENGTH('ClassSessions', 'UpdatedOnUtc') IS NULL
    ALTER TABLE [ClassSessions] ADD [UpdatedOnUtc] datetime2 NOT NULL CONSTRAINT [DF_ClassSessions_UpdatedOnUtc] DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('ClassSessions', 'Deleted') IS NULL
    ALTER TABLE [ClassSessions] ADD [Deleted] bit NOT NULL CONSTRAINT [DF_ClassSessions_Deleted] DEFAULT (0);
IF COL_LENGTH('ClassSessions', 'DeletedOnUtc') IS NULL
    ALTER TABLE [ClassSessions] ADD [DeletedOnUtc] datetime2 NULL;
GO

-- Backfill from the legacy columns (only while they still exist).
IF COL_LENGTH('ClassSessions', 'CreatedOn') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE [ClassSessions] SET
            [CreatedOnUtc] = [CreatedOn],
            [CreatedById]  = TRY_CONVERT(uniqueidentifier, [CreatedBy]),
            [UpdatedOnUtc] = COALESCE([UpdatedOn], [CreatedOn]),
            [UpdatedById]  = TRY_CONVERT(uniqueidentifier, [UpdatedBy]),
            [Deleted]      = [IsDeleted],
            [DeletedOnUtc] = CASE WHEN [IsDeleted] = 1 THEN COALESCE([UpdatedOn], [CreatedOn]) END;');
END;
GO

-- Drop the legacy columns, including their system-named default constraints.
DECLARE @sql nvarchar(max) = N'';
SELECT @sql += N'ALTER TABLE [ClassSessions] DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('ClassSessions')
  AND c.name IN ('IsDeleted', 'CreatedOn', 'CreatedBy', 'UpdatedOn', 'UpdatedBy');
EXEC(@sql);
GO

IF COL_LENGTH('ClassSessions', 'IsDeleted') IS NOT NULL
    ALTER TABLE [ClassSessions] DROP COLUMN [IsDeleted];
IF COL_LENGTH('ClassSessions', 'CreatedOn') IS NOT NULL
    ALTER TABLE [ClassSessions] DROP COLUMN [CreatedOn];
IF COL_LENGTH('ClassSessions', 'CreatedBy') IS NOT NULL
    ALTER TABLE [ClassSessions] DROP COLUMN [CreatedBy];
IF COL_LENGTH('ClassSessions', 'UpdatedOn') IS NOT NULL
    ALTER TABLE [ClassSessions] DROP COLUMN [UpdatedOn];
IF COL_LENGTH('ClassSessions', 'UpdatedBy') IS NOT NULL
    ALTER TABLE [ClassSessions] DROP COLUMN [UpdatedBy];
GO
