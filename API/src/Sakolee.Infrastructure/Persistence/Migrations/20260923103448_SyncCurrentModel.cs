using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "CreatedById",
            //    table: "ClassSessions");

            //migrationBuilder.DropColumn(
            //    name: "CreatedOnUtc",
            //    table: "ClassSessions");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedById",
            //    table: "ClassSessions");

            //migrationBuilder.RenameColumn(
            //    name: "UpdatedOnUtc",
            //    table: "ClassSessions",
            //    newName: "CreatedOn");

            //migrationBuilder.RenameColumn(
            //    name: "DeletedOnUtc",
            //    table: "ClassSessions",
            //    newName: "UpdatedOn");

            //migrationBuilder.RenameColumn(
            //    name: "Deleted",
            //    table: "ClassSessions",
            //    newName: "IsDeleted");

            //migrationBuilder.AddColumn<string>(
            //    name: "CreatedBy",
            //    table: "ClassSessions",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "UpdatedBy",
            //    table: "ClassSessions",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "BillingCycle",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
            //        TenentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
            //        CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
            //        UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
            //        Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BillingCycle", x => x.Id);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_BillingCycle_TenentId_Name",
            //    table: "BillingCycle",
            //    columns: new[] { "TenentId", "Name" },
            //    unique: true,
            //    filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingCycle");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ClassSessions");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "ClassSessions",
                newName: "DeletedOnUtc");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "ClassSessions",
                newName: "Deleted");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "ClassSessions",
                newName: "UpdatedOnUtc");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ClassSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "ClassSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "ClassSessions",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
