using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameFamilyPersonMappingParentIdToFamilyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyPersonMapping_Families_ParentId",
                table: "FamilyPersonMapping");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "FamilyPersonMapping",
                newName: "FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyPersonMapping_ParentId",
                table: "FamilyPersonMapping",
                newName: "IX_FamilyPersonMapping_FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyPersonMapping_Families_FamilyId",
                table: "FamilyPersonMapping",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyPersonMapping_Families_FamilyId",
                table: "FamilyPersonMapping");

            migrationBuilder.RenameColumn(
                name: "FamilyId",
                table: "FamilyPersonMapping",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyPersonMapping_FamilyId",
                table: "FamilyPersonMapping",
                newName: "IX_FamilyPersonMapping_ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyPersonMapping_Families_ParentId",
                table: "FamilyPersonMapping",
                column: "ParentId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
