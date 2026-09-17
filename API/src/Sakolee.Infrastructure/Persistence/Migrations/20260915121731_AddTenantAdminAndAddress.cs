using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAdminAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_AddressId",
                table: "Tenants",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PersonId",
                table: "Tenants",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Addresses_AddressId",
                table: "Tenants",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Persons_PersonId",
                table: "Tenants",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Addresses_AddressId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Persons_PersonId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_AddressId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_PersonId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Tenants");
        }
    }
}
