-- Removes the legacy inline address columns from Families — Address1, Address2, City, State, ZipCode. The
-- household address now lives on the Addresses table, linked by Families.AddressId (see FamilyConfiguration).
--
-- Run AFTER 20261007000100_FamilyAddressToAddresses.sql, which copies those columns into Addresses. Before
-- dropping anything this script checks that:
--   * Families.AddressId exists (the copy script has run), and
--   * every family with a value in any of the five columns is linked to an address.
-- If either check fails it raises an error and drops nothing, so no address data is lost.
--
-- Each column is dropped on its own (with any default constraint or index on it first), so a database where
-- only some of them remain is handled too.
-- Idempotent: safe to run more than once; a no-op once the columns are gone.
SET QUOTED_IDENTIFIER ON;
GO

SET NOEXEC OFF;
GO

-- 1. Safety checks.
IF COL_LENGTH('Families', 'Address1') IS NOT NULL
   OR COL_LENGTH('Families', 'Address2') IS NOT NULL
   OR COL_LENGTH('Families', 'City') IS NOT NULL
   OR COL_LENGTH('Families', 'State') IS NOT NULL
   OR COL_LENGTH('Families', 'ZipCode') IS NOT NULL
BEGIN
    IF COL_LENGTH('Families', 'AddressId') IS NULL
    BEGIN
        RAISERROR('Families.AddressId does not exist. Run 20261007000100_FamilyAddressToAddresses.sql first; no columns were dropped.', 16, 1);
        SET NOEXEC ON;
    END
END;
GO

-- Count families whose inline address has not been copied. Built from whichever of the five columns still
-- exist (dynamic SQL — a static reference to a column already dropped would fail to compile).
DECLARE @conditions nvarchar(max) = N'';
IF COL_LENGTH('Families', 'Address1') IS NOT NULL SET @conditions += N' OR NULLIF(LTRIM(RTRIM([Address1])), N'''') IS NOT NULL';
IF COL_LENGTH('Families', 'Address2') IS NOT NULL SET @conditions += N' OR NULLIF(LTRIM(RTRIM([Address2])), N'''') IS NOT NULL';
IF COL_LENGTH('Families', 'City') IS NOT NULL SET @conditions += N' OR NULLIF(LTRIM(RTRIM([City])), N'''') IS NOT NULL';
IF COL_LENGTH('Families', 'State') IS NOT NULL SET @conditions += N' OR NULLIF(LTRIM(RTRIM([State])), N'''') IS NOT NULL';
IF COL_LENGTH('Families', 'ZipCode') IS NOT NULL SET @conditions += N' OR [ZipCode] IS NOT NULL';

IF @conditions <> N''
BEGIN
    DECLARE @unlinked int;
    DECLARE @sql nvarchar(max) =
        N'SELECT @n = COUNT(*) FROM [Families] WHERE [AddressId] IS NULL AND (' + STUFF(@conditions, 1, 4, N'') + N');';
    EXEC sp_executesql @sql, N'@n int OUTPUT', @n = @unlinked OUTPUT;

    IF @unlinked > 0
    BEGIN
        RAISERROR('%d family address(es) have not been copied to Addresses. Run 20261007000100_FamilyAddressToAddresses.sql first; no columns were dropped.', 16, 1, @unlinked);
        SET NOEXEC ON;
    END
END;
GO

-- 2. Drop each remaining column, removing any default constraint or index on it first.
DECLARE @column sysname;
DECLARE @stmt nvarchar(max);
DECLARE columns_to_drop CURSOR LOCAL FAST_FORWARD FOR
    SELECT c.name
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'[Families]')
      AND c.name IN (N'Address1', N'Address2', N'City', N'State', N'ZipCode');

OPEN columns_to_drop;
FETCH NEXT FROM columns_to_drop INTO @column;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Default constraint on the column.
    SELECT @stmt = N'ALTER TABLE [Families] DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[Families]') AND c.name = @column;
    IF @stmt IS NOT NULL EXEC sp_executesql @stmt;
    SET @stmt = NULL;

    -- Any index that includes the column.
    SELECT @stmt = STRING_AGG(N'DROP INDEX ' + QUOTENAME(i.name) + N' ON [Families];', N' ')
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(N'[Families]')
      AND i.is_primary_key = 0
      AND EXISTS (
          SELECT 1 FROM sys.index_columns ic
          JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
          WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND c.name = @column);
    IF @stmt IS NOT NULL EXEC sp_executesql @stmt;
    SET @stmt = NULL;

    SET @stmt = N'ALTER TABLE [Families] DROP COLUMN ' + QUOTENAME(@column) + N';';
    EXEC sp_executesql @stmt;
    PRINT N'Dropped Families.' + @column;
    SET @stmt = NULL;

    FETCH NEXT FROM columns_to_drop INTO @column;
END;
CLOSE columns_to_drop;
DEALLOCATE columns_to_drop;
GO

SET NOEXEC OFF;
GO
