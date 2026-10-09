-- Adds the EPaymentSchedule master table (see EPaymentScheduleConfiguration). The entity and configuration
-- were added without a script, so every query on it failed with "Invalid object name 'EPaymentSchedule'".
-- Shape follows the configuration and the other hand-built master tables (AccountType etc.): Id, TenentId and
-- the audit user ids are nvarchar(450) (EF converts the Guids to strings), the tenant column is spelled
-- [TenentId], and Name is unique per tenant among live rows.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[EPaymentSchedule]', N'U') IS NULL
BEGIN
    CREATE TABLE [EPaymentSchedule] (
        [Id] nvarchar(450) NOT NULL,
        [TenentId] nvarchar(450) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Active] bit NOT NULL CONSTRAINT [DF_EPaymentSchedule_Active] DEFAULT CAST(1 AS bit),
        [CreatedById] nvarchar(450) NULL,
        [CreatedOnUtc] datetime2(6) NOT NULL CONSTRAINT [DF_EPaymentSchedule_CreatedOnUtc] DEFAULT SYSUTCDATETIME(),
        [UpdatedById] nvarchar(450) NULL,
        [UpdatedOnUtc] datetime2(6) NOT NULL CONSTRAINT [DF_EPaymentSchedule_UpdatedOnUtc] DEFAULT SYSUTCDATETIME(),
        [Deleted] bit NOT NULL CONSTRAINT [DF_EPaymentSchedule_Deleted] DEFAULT CAST(0 AS bit),
        [DeletedOnUtc] datetime2(6) NULL,
        CONSTRAINT [PK_EPaymentSchedule] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EPaymentSchedule_TenentId_Name' AND object_id = OBJECT_ID('EPaymentSchedule'))
BEGIN
    CREATE UNIQUE INDEX [IX_EPaymentSchedule_TenentId_Name] ON [EPaymentSchedule] ([TenentId], [Name]) WHERE [Deleted] = 0;
END;
GO
