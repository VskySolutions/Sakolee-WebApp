using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Creates the master tables the model maps but no earlier migration actually creates:
    /// <c>BillingMethod</c> (its CreateTable in AddFamilyRelation is commented out) and <c>TShirtSize</c>
    /// (AddTShirtSize's body is commented out). Some databases already have them, created by hand, so
    /// every statement only runs when the table/index/constraint is missing.
    /// </summary>
    public partial class AddMissingMasterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[BillingMethod]', N'U') IS NULL
BEGIN
    CREATE TABLE [BillingMethod] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [Name] nvarchar(max) NOT NULL,
        [Active] bit NOT NULL,
        [CreatedById] uniqueidentifier NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [UpdatedById] uniqueidentifier NULL,
        [UpdatedOnUtc] datetime2 NOT NULL,
        [Deleted] bit NOT NULL,
        [DeletedOnUtc] datetime2 NULL,
        CONSTRAINT [PK_BillingMethod] PRIMARY KEY ([Id])
    );
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_BillingMethod_TenantId' AND [object_id] = OBJECT_ID(N'[BillingMethod]'))
    CREATE INDEX [IX_BillingMethod_TenantId] ON [BillingMethod] ([TenantId]);");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[FK_BillingMethod_Tenants_TenantId]', N'F') IS NULL
    ALTER TABLE [BillingMethod] ADD CONSTRAINT [FK_BillingMethod_Tenants_TenantId]
        FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[TShirtSize]', N'U') IS NULL
BEGIN
    CREATE TABLE [TShirtSize] (
        [Id] nvarchar(450) NOT NULL,
        [TenentId] nvarchar(450) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [CreatedOnUtc] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
        [CreatedById] nvarchar(450) NULL,
        [UpdatedOnUtc] datetime2 NULL,
        [UpdatedById] nvarchar(450) NULL,
        [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_TShirtSize] PRIMARY KEY ([Id])
    );
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_TShirtSize_TenentId_Name' AND [object_id] = OBJECT_ID(N'[TShirtSize]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_TShirtSize_TenentId_Name] ON [TShirtSize] ([TenentId], [Name]) WHERE [Deleted] = 0');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing is dropped: these tables may predate this migration on some databases, and dropping
            // them would lose that data.
        }
    }
}
