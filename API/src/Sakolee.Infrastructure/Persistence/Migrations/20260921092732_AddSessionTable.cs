using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "UpdatedBy",
            //    table: "FamilyStatuses",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
                //name: "UpdatedOn",
                //table: "FamilyStatuses",
                //type: "datetime2",
                //nullable: true);

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanceStyle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_Sessions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            //migrationBuilder.CreateIndex(
            //    name: "IX_FamilyStatuses_TenantId",
            //    table: "FamilyStatuses",
            //    column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TenantId",
                table: "Sessions",
                column: "TenantId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_FamilyStatuses_Tenants_TenantId",
            //    table: "FamilyStatuses",
            //    column: "TenantId",
            //    principalTable: "Tenants",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FamilyStatuses_Tenants_TenantId",
            //    table: "FamilyStatuses");

            migrationBuilder.DropTable(
                name: "Sessions");

            //migrationBuilder.DropIndex(
            //    name: "IX_FamilyStatuses_TenantId",
            //    table: "FamilyStatuses");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedBy",
            //    table: "FamilyStatuses");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedOn",
            //    table: "FamilyStatuses");
        }
    }
}
