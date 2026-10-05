-- Adds TenantRoleOverrides (TenantRoleOverride, see TenantRoleOverrideConfiguration): a tenant's own
-- permission set for a platform role, written when a tenant admin edits that role from the Roles screen.
-- Without this table, logging in fails (the user query includes Role.TenantOverrides).
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[TenantRoleOverrides]', N'U') IS NULL
BEGIN
    CREATE TABLE [TenantRoleOverrides] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [Permissions] nvarchar(4000) NOT NULL,
        [CreatedById] uniqueidentifier NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [UpdatedById] uniqueidentifier NULL,
        [UpdatedOnUtc] datetime2 NOT NULL,
        [Deleted] bit NOT NULL,
        [DeletedOnUtc] datetime2 NULL,
        CONSTRAINT [PK_TenantRoleOverrides] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantRoleOverrides_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TenantRoleOverrides_RoleId' AND object_id = OBJECT_ID('TenantRoleOverrides'))
BEGIN
    CREATE INDEX [IX_TenantRoleOverrides_RoleId] ON [TenantRoleOverrides] ([RoleId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TenantRoleOverrides_TenantId_RoleId' AND object_id = OBJECT_ID('TenantRoleOverrides'))
BEGIN
    CREATE UNIQUE INDEX [IX_TenantRoleOverrides_TenantId_RoleId] ON [TenantRoleOverrides] ([TenantId], [RoleId]) WHERE [Deleted] = 0;
END;
GO
