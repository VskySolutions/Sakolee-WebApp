-- Adds Policy and PolicyClassMapping (see PolicyConfiguration): tenant policies (rich-text content) and the
-- classes each one applies to. Ids follow the app's uniqueidentifier convention and the AuditableEntity
-- audit/soft-delete columns. PolicyClassMapping is a plain join table (hard-deleted rows, no audit columns);
-- Class stores its ids as nvarchar(450), so no FK is declared onto it.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

-- An earlier draft of these tables used bigint ids (Id/TenantId/ClassId) and IsActive/IsDeleted/CreatedDate
-- columns, which cannot hold the app's uniqueidentifier tenant and class ids. Replace that draft — but only
-- while both tables are empty; with data in them, stop rather than drop anything.
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID(N'[Policy]') AND c.name = 'Id' AND t.name = 'bigint'
)
BEGIN
    IF EXISTS (SELECT 1 FROM [Policy])
       OR (OBJECT_ID(N'[PolicyClassMapping]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM [PolicyClassMapping]))
    BEGIN
        RAISERROR('Policy/PolicyClassMapping exist with bigint ids and contain rows; migrate them by hand.', 16, 1);
        SET NOEXEC ON;
    END
    ELSE
    BEGIN
        IF OBJECT_ID(N'[PolicyClassMapping]', N'U') IS NOT NULL DROP TABLE [PolicyClassMapping];
        DROP TABLE [Policy];
    END
END;
GO

IF OBJECT_ID(N'[Policy]', N'U') IS NULL
BEGIN
    CREATE TABLE [Policy] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Content] nvarchar(max) NULL,
        [Active] bit NOT NULL CONSTRAINT [DF_Policy_Active] DEFAULT CAST(1 AS bit),
        [DisplayOrder] int NOT NULL CONSTRAINT [DF_Policy_DisplayOrder] DEFAULT 0,
        [CreatedById] uniqueidentifier NULL,
        [CreatedOnUtc] datetime2 NOT NULL CONSTRAINT [DF_Policy_CreatedOnUtc] DEFAULT SYSUTCDATETIME(),
        [UpdatedById] uniqueidentifier NULL,
        [UpdatedOnUtc] datetime2 NOT NULL,
        [Deleted] bit NOT NULL CONSTRAINT [DF_Policy_Deleted] DEFAULT CAST(0 AS bit),
        [DeletedOnUtc] datetime2 NULL,
        CONSTRAINT [PK_Policy] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Policy_TenantId_Name' AND object_id = OBJECT_ID('Policy'))
BEGIN
    CREATE UNIQUE INDEX [IX_Policy_TenantId_Name] ON [Policy] ([TenantId], [Name]) WHERE [Deleted] = 0;
END;
GO

IF OBJECT_ID(N'[PolicyClassMapping]', N'U') IS NULL
BEGIN
    CREATE TABLE [PolicyClassMapping] (
        [PolicyId] uniqueidentifier NOT NULL,
        [ClassId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_PolicyClassMapping] PRIMARY KEY ([PolicyId], [ClassId]),
        CONSTRAINT [FK_PolicyClass_Policy] FOREIGN KEY ([PolicyId]) REFERENCES [Policy] ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PolicyClassMapping_ClassId' AND object_id = OBJECT_ID('PolicyClassMapping'))
BEGIN
    CREATE INDEX [IX_PolicyClassMapping_ClassId] ON [PolicyClassMapping] ([ClassId]);
END;
GO
SET NOEXEC OFF;
GO
