using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleTenantsPerPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantPersonMapping_PersonId",
                table: "TenantPersonMapping");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPersonMapping_PersonId_TenantId",
                table: "TenantPersonMapping",
                columns: new[] { "PersonId", "TenantId" },
                unique: true,
                filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantPersonMapping_PersonId_TenantId",
                table: "TenantPersonMapping");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPersonMapping_PersonId",
                table: "TenantPersonMapping",
                column: "PersonId",
                unique: true,
                filter: "[Deleted] = 0");
        }
    }
}
