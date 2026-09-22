using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_FamilyStatuses_FamilyStatusId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_FamilyStatusId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FamilyStatusId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "FamilyStatuses");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "FamilyStatuses");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "FamilyStatuses");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "FamilyStatuses");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "FamilyStatuses");

            migrationBuilder.RenameColumn(
                name: "UpdatedOnUtc",
                table: "FamilyStatuses",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "FamilyStatuses",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "FamilyStatuses",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedOnUtc",
                table: "FamilyStatuses",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "FamilyStatuses",
                newName: "CreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "FamilyStatuses",
                newName: "DeletedOnUtc");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "FamilyStatuses",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "FamilyStatuses",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "FamilyStatuses",
                newName: "UpdatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "FamilyStatuses",
                newName: "Code");

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyStatusId",
                table: "Persons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "FamilyStatuses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "FamilyStatuses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "FamilyStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "FamilyStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "FamilyStatuses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FamilyStatusId",
                table: "Persons",
                column: "FamilyStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_FamilyStatuses_FamilyStatusId",
                table: "Persons",
                column: "FamilyStatusId",
                principalTable: "FamilyStatuses",
                principalColumn: "FamilyStatusId");
        }
    }
}
