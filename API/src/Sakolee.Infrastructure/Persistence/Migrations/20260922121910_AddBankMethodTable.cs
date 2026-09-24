using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankMethodTable : Migration
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

            migrationBuilder.CreateTable(
                name: "BankMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankMethods_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankMethods_TenantId",
                table: "BankMethods",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankMethods");

            //migrationBuilder.DropColumn(
            //    name: "CreatedBy",
            //    table: "ClassSessions");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedBy",
            //    table: "ClassSessions");

            //migrationBuilder.RenameColumn(
            //    name: "UpdatedOn",
            //    table: "ClassSessions",
            //    newName: "DeletedOnUtc");

            //migrationBuilder.RenameColumn(
            //    name: "IsDeleted",
            //    table: "ClassSessions",
            //    newName: "Deleted");

            //migrationBuilder.RenameColumn(
            //    name: "CreatedOn",
            //    table: "ClassSessions",
            //    newName: "UpdatedOnUtc");

            //migrationBuilder.AddColumn<Guid>(
            //    name: "CreatedById",
            //    table: "ClassSessions",
            //    type: "uniqueidentifier",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "CreatedOnUtc",
            //    table: "ClassSessions",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<Guid>(
            //    name: "UpdatedById",
            //    table: "ClassSessions",
            //    type: "uniqueidentifier",
            //    nullable: true);
        }
    }
}
