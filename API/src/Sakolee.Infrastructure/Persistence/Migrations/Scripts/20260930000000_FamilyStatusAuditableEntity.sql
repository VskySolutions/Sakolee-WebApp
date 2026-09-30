-- Moves FamilyStatuses onto the standard AuditableEntity columns (FamilyStatus : AuditableEntity):
--   CreatedById / CreatedOnUtc / UpdatedById / UpdatedOnUtc / Deleted / DeletedOnUtc, plus Active.
-- Backfills them from the legacy CreatedBy (nvarchar user id) / CreatedOn / UpdatedBy / UpdatedOn /
-- IsDeleted columns, then drops the legacy columns (EF no longer maps them, and CreatedOn/IsDeleted are
-- NOT NULL, so leaving them would break inserts).
-- Legacy IsDeleted = 1 rows were hidden everywhere by the query filter, so they become Deleted = 1.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('FamilyStatuses', 'CreatedById') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [CreatedById] uniqueidentifier NULL;
IF COL_LENGTH('FamilyStatuses', 'CreatedOnUtc') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [CreatedOnUtc] datetime2 NOT NULL CONSTRAINT [DF_FamilyStatuses_CreatedOnUtc] DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('FamilyStatuses', 'UpdatedById') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [UpdatedById] uniqueidentifier NULL;
IF COL_LENGTH('FamilyStatuses', 'UpdatedOnUtc') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [UpdatedOnUtc] datetime2 NOT NULL CONSTRAINT [DF_FamilyStatuses_UpdatedOnUtc] DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('FamilyStatuses', 'Deleted') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [Deleted] bit NOT NULL CONSTRAINT [DF_FamilyStatuses_Deleted] DEFAULT (0);
IF COL_LENGTH('FamilyStatuses', 'DeletedOnUtc') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [DeletedOnUtc] datetime2 NULL;
IF COL_LENGTH('FamilyStatuses', 'Active') IS NULL
    ALTER TABLE [FamilyStatuses] ADD [Active] bit NOT NULL CONSTRAINT [DF_FamilyStatuses_Active] DEFAULT (1);
GO

-- Backfill from the legacy columns (only while they still exist).
IF COL_LENGTH('FamilyStatuses', 'CreatedOn') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE [FamilyStatuses] SET
            [CreatedOnUtc] = [CreatedOn],
            [CreatedById]  = TRY_CONVERT(uniqueidentifier, [CreatedBy]),
            [UpdatedOnUtc] = COALESCE([UpdatedOn], [CreatedOn]),
            [UpdatedById]  = TRY_CONVERT(uniqueidentifier, [UpdatedBy]),
            [Deleted]      = [IsDeleted],
            [DeletedOnUtc] = CASE WHEN [IsDeleted] = 1 THEN COALESCE([UpdatedOn], [CreatedOn]) END;');
END;
GO

-- Drop the legacy columns.
IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_FamilyStatuses_IsDeleted')
    ALTER TABLE [FamilyStatuses] DROP CONSTRAINT [DF_FamilyStatuses_IsDeleted];
IF COL_LENGTH('FamilyStatuses', 'IsDeleted') IS NOT NULL
    ALTER TABLE [FamilyStatuses] DROP COLUMN [IsDeleted];
IF COL_LENGTH('FamilyStatuses', 'CreatedOn') IS NOT NULL
    ALTER TABLE [FamilyStatuses] DROP COLUMN [CreatedOn];
IF COL_LENGTH('FamilyStatuses', 'CreatedBy') IS NOT NULL
    ALTER TABLE [FamilyStatuses] DROP COLUMN [CreatedBy];
IF COL_LENGTH('FamilyStatuses', 'UpdatedOn') IS NOT NULL
    ALTER TABLE [FamilyStatuses] DROP COLUMN [UpdatedOn];
IF COL_LENGTH('FamilyStatuses', 'UpdatedBy') IS NOT NULL
    ALTER TABLE [FamilyStatuses] DROP COLUMN [UpdatedBy];
GO
