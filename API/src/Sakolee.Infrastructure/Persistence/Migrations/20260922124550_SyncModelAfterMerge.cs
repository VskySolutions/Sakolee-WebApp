using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "ClassSessions");

            migrationBuilder.RenameColumn(
                name: "UpdatedOnUtc",
                table: "ClassSessions",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedOnUtc",
                table: "ClassSessions",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "Deleted",
                table: "ClassSessions",
                newName: "IsDeleted");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ClassSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ClassSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
