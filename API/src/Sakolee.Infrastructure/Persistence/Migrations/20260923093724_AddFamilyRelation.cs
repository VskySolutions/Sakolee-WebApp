using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "BankMethods");

            //migrationBuilder.CreateTable(
            //    name: "BillingMethod",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Active = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Deleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BillingMethod", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_BillingMethod_Tenants_TenantId",
            //            column: x => x.TenantId,
            //            principalTable: "Tenants",
            //            principalColumn: "Id");
            //    });

            migrationBuilder.CreateTable(
                name: "FamilyRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyRelations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            //migrationBuilder.CreateIndex(
            //    name: "IX_BillingMethod_TenantId",
            //    table: "BillingMethod",
            //    column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelations_TenantId",
                table: "FamilyRelations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "BillingMethod");

            migrationBuilder.DropTable(
                name: "FamilyRelations");

            //migrationBuilder.CreateTable(
            //    name: "BankMethods",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Active = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Deleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BankMethods", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_BankMethods_Tenants_TenantId",
            //            column: x => x.TenantId,
            //            principalTable: "Tenants",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_BankMethods_TenantId",
            //    table: "BankMethods",
            //    column: "TenantId");
        }
    }
}
