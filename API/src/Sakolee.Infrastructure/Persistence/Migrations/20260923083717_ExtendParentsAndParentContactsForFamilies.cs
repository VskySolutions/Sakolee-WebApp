using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Extends the pre-existing legacy <c>Parents</c>/<c>ParentContacts</c> tables (managed outside
    /// this project's original migration history — see <c>FamilyConfiguration</c>) with the columns the
    /// Families feature needs, the same way <c>AddStudentColumnsForPrototypeForm</c> extended the
    /// legacy <c>Students</c> table. Deliberately hand-written as <c>AddColumn</c>/<c>DropColumn</c>
    /// rather than the scaffolded <c>CreateTable</c> — EF had no prior model for these tables, so it
    /// scaffolded them as brand new; both tables already exist.
    /// </remarks>
    public partial class ExtendParentsAndParentContactsForFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Parents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "StudioLocationId",
                table: "Parents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyStatusId",
                table: "Parents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Parents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralName",
                table: "Parents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPerson",
                table: "Parents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyPhone",
                table: "Parents",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthInsuranceCarrier",
                table: "Parents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "ParentContacts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Relation",
                table: "ParentContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillingContact",
                table: "ParentContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorizedToPickUpStudent",
                table: "ParentContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Parents_TenantId",
                table: "Parents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_TenantId_FamilyName",
                table: "Parents",
                columns: new[] { "TenantId", "FamilyName" });

            migrationBuilder.CreateIndex(
                name: "IX_Parents_StudioLocationId",
                table: "Parents",
                column: "StudioLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_FamilyStatusId",
                table: "Parents",
                column: "FamilyStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Tenants_TenantId",
                table: "Parents",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Locations_StudioLocationId",
                table: "Parents",
                column: "StudioLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_FamilyStatuses_FamilyStatusId",
                table: "Parents",
                column: "FamilyStatusId",
                principalTable: "FamilyStatuses",
                principalColumn: "FamilyStatusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Parents_Tenants_TenantId", table: "Parents");
            migrationBuilder.DropForeignKey(name: "FK_Parents_Locations_StudioLocationId", table: "Parents");
            migrationBuilder.DropForeignKey(name: "FK_Parents_FamilyStatuses_FamilyStatusId", table: "Parents");

            migrationBuilder.DropIndex(name: "IX_Parents_TenantId", table: "Parents");
            migrationBuilder.DropIndex(name: "IX_Parents_TenantId_FamilyName", table: "Parents");
            migrationBuilder.DropIndex(name: "IX_Parents_StudioLocationId", table: "Parents");
            migrationBuilder.DropIndex(name: "IX_Parents_FamilyStatusId", table: "Parents");

            migrationBuilder.DropColumn(name: "TenantId", table: "Parents");
            migrationBuilder.DropColumn(name: "StudioLocationId", table: "Parents");
            migrationBuilder.DropColumn(name: "FamilyStatusId", table: "Parents");
            migrationBuilder.DropColumn(name: "Source", table: "Parents");
            migrationBuilder.DropColumn(name: "ReferralName", table: "Parents");
            migrationBuilder.DropColumn(name: "EmergencyContactPerson", table: "Parents");
            migrationBuilder.DropColumn(name: "EmergencyPhone", table: "Parents");
            migrationBuilder.DropColumn(name: "HealthInsuranceCarrier", table: "Parents");

            migrationBuilder.DropColumn(name: "PersonId", table: "ParentContacts");
            migrationBuilder.DropColumn(name: "Relation", table: "ParentContacts");
            migrationBuilder.DropColumn(name: "IsBillingContact", table: "ParentContacts");
            migrationBuilder.DropColumn(name: "IsAuthorizedToPickUpStudent", table: "ParentContacts");
        }
    }
}
