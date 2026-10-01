-- Adds Tenants.TenantLogoMediaId (Tenant.TenantLogoMediaId -> Media, see TenantConfiguration).
-- The entity/configuration change landed in 56acdb1 without a migration; without this column every
-- query that loads a Tenant (e.g. GET /api/auth/profile) fails with "Invalid column name".
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('Tenants', 'TenantLogoMediaId') IS NULL
BEGIN
    ALTER TABLE [Tenants] ADD [TenantLogoMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tenants_TenantLogoMediaId' AND object_id = OBJECT_ID('Tenants'))
BEGIN
    CREATE INDEX [IX_Tenants_TenantLogoMediaId] ON [Tenants] ([TenantLogoMediaId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Tenants_Media_TenantLogoMediaId')
BEGIN
    ALTER TABLE [Tenants] ADD CONSTRAINT [FK_Tenants_Media_TenantLogoMediaId]
        FOREIGN KEY ([TenantLogoMediaId]) REFERENCES [Media] ([Id]);
END;
GO
