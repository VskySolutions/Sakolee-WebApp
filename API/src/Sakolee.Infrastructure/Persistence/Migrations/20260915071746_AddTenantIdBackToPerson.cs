using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdBackToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Persons_PartyType_LastName",
                table: "Persons",
                columns: new[] { "PartyType", "LastName" });
        }
    }
}
