-- Gives every tenant-owned master table the columns tenant provisioning needs to seed default rows:
--   * Code     (nvarchar(100) NULL) — the stable key a seeded row is matched on. Unlike Name it survives a
--     rename, so re-running the provisioner never duplicates a row the tenant has relabelled. NULL for
--     rows a tenant adds themselves.
--   * IsSystem (bit NOT NULL, default 0) — marks a seeded row the application depends on, so it can be
--     protected from delete / re-code. Existing rows default to 0 (fully editable, as today).
--   * UX_<Table>_TenantId_Code — one Code per tenant among live rows (filtered, so NULL codes and
--     soft-deleted rows never collide).
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

DECLARE @table sysname, @sql nvarchar(max), @filter nvarchar(200), @tenantColumn sysname;
DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT [Name] FROM @tables;
OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @table;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(@table), 'U') IS NULL
    BEGIN
        PRINT 'Skipped ' + @table + ' (table not found).';
    END
    ELSE
    BEGIN
        IF COL_LENGTH(@table, 'Code') IS NULL
        BEGIN
            SET @sql = N'ALTER TABLE ' + QUOTENAME(@table) + N' ADD [Code] nvarchar(100) NULL;';
            EXEC sp_executesql @sql;
        END;

        IF COL_LENGTH(@table, 'IsSystem') IS NULL
        BEGIN
            SET @sql = N'ALTER TABLE ' + QUOTENAME(@table) + N' ADD [IsSystem] bit NOT NULL'
                     + N' CONSTRAINT ' + QUOTENAME('DF_' + @table + '_IsSystem') + N' DEFAULT CAST(0 AS bit);';
            EXEC sp_executesql @sql;
        END;

        -- AccountType, BillingCycle, ClassCategory, HearAboutUs and TShirtSize store the tenant in a
        -- column spelled [TenentId] (nvarchar(450)); their EF configurations map TenantId onto it.
        SET @tenantColumn = CASE
            WHEN COL_LENGTH(@table, 'TenantId') IS NOT NULL THEN N'TenantId'
            WHEN COL_LENGTH(@table, 'TenentId') IS NOT NULL THEN N'TenentId'
        END;

        IF @tenantColumn IS NULL
        BEGIN
            PRINT 'WARNING: ' + @table + ' has no TenantId/TenentId column - UX_' + @table + '_TenantId_Code not created.';
        END
        ELSE IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE object_id = OBJECT_ID(QUOTENAME(@table)) AND name = 'UX_' + @table + '_TenantId_Code'
        )
        BEGIN
            -- Soft-deleted rows are left out of the uniqueness check where the table has a Deleted flag.
            SET @filter = N'[Code] IS NOT NULL'
                        + CASE WHEN COL_LENGTH(@table, 'Deleted') IS NOT NULL THEN N' AND [Deleted] = 0' ELSE N'' END;
            SET @sql = N'CREATE UNIQUE INDEX ' + QUOTENAME('UX_' + @table + '_TenantId_Code')
                     + N' ON ' + QUOTENAME(@table) + N' (' + QUOTENAME(@tenantColumn) + N', [Code]) WHERE ' + @filter + N';';
            EXEC sp_executesql @sql;
        END;

        PRINT 'Updated ' + @table + '.';
    END;

    FETCH NEXT FROM table_cursor INTO @table;
END;

CLOSE table_cursor;
DEALLOCATE table_cursor;
GO
