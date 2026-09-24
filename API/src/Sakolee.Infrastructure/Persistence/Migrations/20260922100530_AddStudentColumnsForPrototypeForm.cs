using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentColumnsForPrototypeForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllergiesNotes",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowTextMessaging",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "DisabilitiesNotes",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasImmunizations",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthInsuranceCarrier",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllergiesNotes",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AllowTextMessaging",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DisabilitiesNotes",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "HasImmunizations",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "HealthInsuranceCarrier",
                table: "Students");
        }
    }
}
