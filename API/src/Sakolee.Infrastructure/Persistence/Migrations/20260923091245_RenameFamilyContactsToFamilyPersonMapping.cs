using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Renames <c>FamilyContacts</c> to <c>FamilyPersonMapping</c> (matching the
    /// <see cref="Sakolee.Domain.Entities.TenantPersonMapping"/>/<c>TenantPersonMapping</c> naming
    /// convention already used for Tenant/Person's own join table). Hand-written as
    /// <c>RenameTable</c>/<c>RenameIndex</c>/<c>AddForeignKey</c> rather than the scaffolded
    /// <c>DropTable</c>/<c>CreateTable</c> — EF scaffolds a full recreate whenever the CLR entity's own
    /// name changes (unlike the Parents→Families rename, where the CLR type <c>Family</c> stayed put and
    /// only its <c>ToTable</c> call changed), but a recreate is unnecessary and would data-loss-warn.
    /// </remarks>
    public partial class RenameFamilyContactsToFamilyPersonMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyContacts_Families_ParentId",
                table: "FamilyContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FamilyContacts",
                table: "FamilyContacts");

            migrationBuilder.RenameTable(
                name: "FamilyContacts",
                newName: "FamilyPersonMapping");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyContacts_ParentId",
                table: "FamilyPersonMapping",
                newName: "IX_FamilyPersonMapping_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FamilyPersonMapping",
                table: "FamilyPersonMapping",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyPersonMapping_Families_ParentId",
                table: "FamilyPersonMapping",
                column: "ParentId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyPersonMapping_Families_ParentId",
                table: "FamilyPersonMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FamilyPersonMapping",
                table: "FamilyPersonMapping");

            migrationBuilder.RenameTable(
                name: "FamilyPersonMapping",
                newName: "FamilyContacts");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyPersonMapping_ParentId",
                table: "FamilyContacts",
                newName: "IX_FamilyContacts_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FamilyContacts",
                table: "FamilyContacts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyContacts_Families_ParentId",
                table: "FamilyContacts",
                column: "ParentId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
