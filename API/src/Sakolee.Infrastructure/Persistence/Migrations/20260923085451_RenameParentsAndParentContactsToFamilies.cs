using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Renames the legacy <c>Parents</c>/<c>ParentContacts</c> tables to <c>Families</c>/
    /// <c>FamilyContacts</c>. EF tracks and renames the FKs/indexes it knows about (all handled by the
    /// scaffolded calls below); <c>FK_Students_Parents</c> is not EF-tracked
    /// (<see cref="Sakolee.Domain.Entities.Student.ParentId"/> carries no configured relationship — see
    /// its remarks) and — unlike <c>FK_ParentContacts_Parents</c> — references <c>Parents</c>'
    /// PRIMARY KEY, which cannot be dropped while any FK still points at it. It is therefore dropped by
    /// hand up front (before <c>PK_Parents</c> goes) and recreated by hand at the end, against the
    /// renamed table, rather than renamed in place.
    /// </remarks>
    public partial class RenameParentsAndParentContactsToFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Must go before DropPrimaryKey(PK_Parents) below — see the class remarks.
            migrationBuilder.Sql("ALTER TABLE [Students] DROP CONSTRAINT [FK_Students_Parents];");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentContacts_Parents",
                table: "ParentContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_FamilyStatuses_FamilyStatusId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Locations_StudioLocationId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Tenants_TenantId",
                table: "Parents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Parents",
                table: "Parents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentContacts",
                table: "ParentContacts");

            migrationBuilder.RenameTable(
                name: "Parents",
                newName: "Families");

            migrationBuilder.RenameTable(
                name: "ParentContacts",
                newName: "FamilyContacts");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_TenantId_FamilyName",
                table: "Families",
                newName: "IX_Families_TenantId_FamilyName");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_TenantId",
                table: "Families",
                newName: "IX_Families_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_StudioLocationId",
                table: "Families",
                newName: "IX_Families_StudioLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_FamilyStatusId",
                table: "Families",
                newName: "IX_Families_FamilyStatusId");

            // Not a rename: IX_ParentContacts_ParentId was never actually created (an oversight in
            // ExtendParentsAndParentContactsForFamilies) — created fresh here under its final name.
            migrationBuilder.CreateIndex(
                name: "IX_FamilyContacts_ParentId",
                table: "FamilyContacts",
                column: "ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Families",
                table: "Families",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FamilyContacts",
                table: "FamilyContacts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_FamilyStatuses_FamilyStatusId",
                table: "Families",
                column: "FamilyStatusId",
                principalTable: "FamilyStatuses",
                principalColumn: "FamilyStatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Locations_StudioLocationId",
                table: "Families",
                column: "StudioLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Tenants_TenantId",
                table: "Families",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyContacts_Families_ParentId",
                table: "FamilyContacts",
                column: "ParentId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Recreated against the renamed table — see the class remarks.
            migrationBuilder.Sql("ALTER TABLE [Students] ADD CONSTRAINT [FK_Students_Families] FOREIGN KEY ([ParentId]) REFERENCES [Families] ([Id]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Must go before the table/PK are renamed back below — see the class remarks.
            migrationBuilder.Sql("ALTER TABLE [Students] DROP CONSTRAINT [FK_Students_Families];");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_FamilyStatuses_FamilyStatusId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_Locations_StudioLocationId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_Tenants_TenantId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyContacts_Families_ParentId",
                table: "FamilyContacts");

            // Not a rename in reverse either — see the matching comment in Up(): this index never
            // existed before this migration, so reversing it means dropping it, not renaming it back.
            migrationBuilder.DropIndex(
                name: "IX_FamilyContacts_ParentId",
                table: "FamilyContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FamilyContacts",
                table: "FamilyContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Families",
                table: "Families");

            migrationBuilder.RenameTable(
                name: "FamilyContacts",
                newName: "ParentContacts");

            migrationBuilder.RenameTable(
                name: "Families",
                newName: "Parents");

            migrationBuilder.RenameIndex(
                name: "IX_Families_TenantId_FamilyName",
                table: "Parents",
                newName: "IX_Parents_TenantId_FamilyName");

            migrationBuilder.RenameIndex(
                name: "IX_Families_TenantId",
                table: "Parents",
                newName: "IX_Parents_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_StudioLocationId",
                table: "Parents",
                newName: "IX_Parents_StudioLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_FamilyStatusId",
                table: "Parents",
                newName: "IX_Parents_FamilyStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentContacts",
                table: "ParentContacts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Parents",
                table: "Parents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentContacts_Parents",
                table: "ParentContacts",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_FamilyStatuses_FamilyStatusId",
                table: "Parents",
                column: "FamilyStatusId",
                principalTable: "FamilyStatuses",
                principalColumn: "FamilyStatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Locations_StudioLocationId",
                table: "Parents",
                column: "StudioLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Tenants_TenantId",
                table: "Parents",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("ALTER TABLE [Students] ADD CONSTRAINT [FK_Students_Parents] FOREIGN KEY ([ParentId]) REFERENCES [Parents] ([Id]);");
        }
    }
}
