using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveTenantOffPersonToMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Tenants_TenantId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_TenantId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_TenantId_PartyType_LastName",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Persons");

            migrationBuilder.CreateTable(
                name: "TenantPersonMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPersonMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPersonMapping_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantPersonMapping_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_PartyType_LastName",
                table: "Persons",
                columns: new[] { "PartyType", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantPersonMapping_PersonId",
                table: "TenantPersonMapping",
                column: "PersonId",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPersonMapping_TenantId",
                table: "TenantPersonMapping",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantPersonMapping");

            migrationBuilder.DropIndex(
                name: "IX_Persons_PartyType_LastName",
                table: "Persons");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Persons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TenantId",
                table: "Persons",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TenantId_PartyType_LastName",
                table: "Persons",
                columns: new[] { "TenantId", "PartyType", "LastName" });

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Tenants_TenantId",
                table: "Persons",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
