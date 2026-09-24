using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncSakoleeDbModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_BillingMethod_TenantId_Name",
            //    table: "BillingMethod");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "BillingCycle",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_BillingCycle",
            //    table: "BillingCycle",
            //    column: "Id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_BillingCycle_TenentId_Name",
            //    table: "BillingCycle",
            //    columns: new[] { "TenentId", "Name" },
            //    unique: true,
            //    filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_BillingCycle",
            //    table: "BillingCycle");

            //migrationBuilder.DropIndex(
            //    name: "IX_BillingCycle_TenentId_Name",
            //    table: "BillingCycle");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "BillingCycle",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            //migrationBuilder.CreateIndex(
            //    name: "IX_BillingMethod_TenantId_Name",
            //    table: "BillingMethod",
            //    columns: new[] { "TenantId", "Name" },
            //    unique: true,
            //    filter: "[Deleted] = 0");
        }
    }
}
