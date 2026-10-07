-- Moves the family's household address off the Families row and onto the shared Addresses table, the same
-- way Tenants holds its address (Tenants.AddressId → Addresses.Id). See FamilyConfiguration.
--   1. Adds Families.AddressId (uniqueidentifier NULL, FK → Addresses.Id, no cascade).
--   2. Copies each family's inline address (Address1/Address2/City/State/ZipCode) into a new Addresses row
--      (AddressType 'Home'; City → CityName, State → StateName, ZipCode → PostalCode) and links it.
--      Country is left empty — the legacy columns never recorded one.
-- The inline columns are NOT dropped here — that is 20261007000200_DropFamilyInlineAddressColumns.sql, run
-- after this one once the copied addresses have been checked.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Families.AddressId
IF COL_LENGTH('Families', 'AddressId') IS NULL
BEGIN
    ALTER TABLE [Families] ADD [AddressId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Families_AddressId' AND object_id = OBJECT_ID('Families'))
BEGIN
    CREATE INDEX [IX_Families_AddressId] ON [Families] ([AddressId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Families_Addresses_AddressId')
BEGIN
    ALTER TABLE [Families] ADD CONSTRAINT [FK_Families_Addresses_AddressId]
        FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]);
END;
GO

-- 2. Copy the inline address into Addresses. Dynamic SQL: on a re-run after step 3 the legacy columns no
--    longer exist, and a static reference to them would fail to compile.
IF COL_LENGTH('Families', 'Address1') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
        DECLARE @map TABLE (FamilyId nvarchar(450) NOT NULL, AddressId uniqueidentifier NOT NULL);

        INSERT INTO @map (FamilyId, AddressId)
        SELECT f.[Id], NEWID()
        FROM [Families] f
        WHERE f.[AddressId] IS NULL
          AND (NULLIF(LTRIM(RTRIM(f.[Address1])), N'''') IS NOT NULL
            OR NULLIF(LTRIM(RTRIM(f.[Address2])), N'''') IS NOT NULL
            OR NULLIF(LTRIM(RTRIM(f.[City])), N'''') IS NOT NULL
            OR NULLIF(LTRIM(RTRIM(f.[State])), N'''') IS NOT NULL
            OR f.[ZipCode] IS NOT NULL);

        INSERT INTO [Addresses] ([Id], [AddressType], [AddressLine1], [AddressLine2], [StateName], [CityName],
                                 [PostalCode], [CreatedById], [CreatedOnUtc], [UpdatedById], [UpdatedOnUtc], [Deleted])
        SELECT m.AddressId, N''Home'',
               LEFT(NULLIF(LTRIM(RTRIM(f.[Address1])), N''''), 256),
               LEFT(NULLIF(LTRIM(RTRIM(f.[Address2])), N''''), 256),
               LEFT(NULLIF(LTRIM(RTRIM(f.[State])), N''''), 100),
               LEFT(NULLIF(LTRIM(RTRIM(f.[City])), N''''), 100),
               CAST(f.[ZipCode] AS nvarchar(20)),
               TRY_CONVERT(uniqueidentifier, f.[CreatedById]), SYSUTCDATETIME(),
               TRY_CONVERT(uniqueidentifier, f.[UpdatedById]), SYSUTCDATETIME(), 0
        FROM @map m
        JOIN [Families] f ON f.[Id] = m.FamilyId;

        UPDATE f SET f.[AddressId] = m.AddressId
        FROM [Families] f
        JOIN @map m ON m.FamilyId = f.[Id];';
END;
GO

