using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTShirtSizeAuditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "TShirtSize",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE TShirtSize
                SET UpdatedOnUtc = CreatedOnUtc
                WHERE UpdatedOnUtc IS NULL
            """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "TShirtSize",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "TShirtSize",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "TShirtSize");
        }
    }
}