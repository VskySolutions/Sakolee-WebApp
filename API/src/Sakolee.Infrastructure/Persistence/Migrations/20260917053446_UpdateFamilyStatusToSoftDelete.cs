using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFamilyStatusToSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.DropColumn(
                name: "Status",
                table: "FamilyStatuses");*

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FamilyStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FamilyStatuses");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FamilyStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
