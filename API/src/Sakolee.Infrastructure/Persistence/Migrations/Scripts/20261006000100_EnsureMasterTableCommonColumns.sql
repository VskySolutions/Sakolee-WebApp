-- Brings every tenant-owned master table in line with the columns its entity maps (Active + the
-- AuditableEntity columns). Some of these tables were created before those columns existed and no
-- migration or script ever added them; any query on such a table then fails with
-- "Invalid column name 'Active' / 'DeletedOnUtc'" — first seen in TenantMasterDataProvisioner, which reads
-- every master table when a tenant is created.
--   * Active       bit NOT NULL, default 1 — existing rows stay usable.
--   * Deleted      bit NOT NULL, default 0 — existing rows stay live.
--   * DeletedOnUtc datetime2 NULL
--   * CreatedById / UpdatedById uniqueidentifier NULL
--   * CreatedOnUtc / UpdatedOnUtc datetime2 NOT NULL — existing rows are stamped with the time of this run.
-- Tables that do not exist in this database are skipped.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @tables TABLE ([Name] sysname NOT NULL);
INSERT INTO @tables ([Name]) VALUES
    ('AccountType'),
    ('BillingCycle'),
    ('BillingMethod'),
    ('ClassCategory'),
    ('ClassRooms'),
    ('FamilyStatuses'),
    ('HearAboutUs'),
    ('Locations'),
    ('MembershipType'),
    ('StudentGradeLevel'),
    ('TShirtSize');

-- Column name -> definition used when it is missing. Defaults are named DF_<Table>_<Column>.
DECLARE @columns TABLE ([Name] sysname NOT NULL, [Definition] nvarchar(200) NOT NULL, [Default] nvarchar(100) NULL);
INSERT INTO @columns ([Name], [Definition], [Default]) VALUES
    ('Active',       'bit NOT NULL',              'CAST(1 AS bit)'),
    ('Deleted',      'bit NOT NULL',              'CAST(0 AS bit)'),
    ('DeletedOnUtc', 'datetime2 NULL',            NULL),
    ('CreatedById',  'uniqueidentifier NULL',     NULL),
    ('CreatedOnUtc', 'datetime2 NOT NULL',        'SYSUTCDATETIME()'),
    ('UpdatedById',  'uniqueidentifier NULL',     NULL),
    ('UpdatedOnUtc', 'datetime2 NOT NULL',        'SYSUTCDATETIME()');

DECLARE @table sysname, @column sysname, @definition nvarchar(200), @default nvarchar(100), @sql nvarchar(max);
DECLARE work_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.[Name], c.[Name], c.[Definition], c.[Default] FROM @tables t CROSS JOIN @columns c;
OPEN work_cursor;
FETCH NEXT FROM work_cursor INTO @table, @column, @definition, @default;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(@table), 'U') IS NOT NULL AND COL_LENGTH(@table, @column) IS NULL
    BEGIN
        SET @sql = N'ALTER TABLE ' + QUOTENAME(@table) + N' ADD ' + QUOTENAME(@column) + N' ' + @definition
                 + CASE WHEN @default IS NULL THEN N''
                        ELSE N' CONSTRAINT ' + QUOTENAME('DF_' + @table + '_' + @column) + N' DEFAULT ' + @default END
                 + N';';
        EXEC sp_executesql @sql;
        PRINT 'Added ' + @table + '.' + @column;
    END;

    FETCH NEXT FROM work_cursor INTO @table, @column, @definition, @default;
END;

CLOSE work_cursor;
DEALLOCATE work_cursor;
GO
