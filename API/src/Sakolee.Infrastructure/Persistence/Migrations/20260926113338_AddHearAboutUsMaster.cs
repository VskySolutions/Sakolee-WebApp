using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHearAboutUsMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "HearAboutUs",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
            //        TenentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
            //        CreatedOnUtc = table.Column<DateTime>(type: "datetime2(6)", precision: 6, nullable: false, defaultValueSql: "sysutcdatetime()"),
            //        UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
            //        UpdatedOnUtc = table.Column<DateTime>(type: "datetime2(6)", precision: 6, nullable: false),
            //        Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
            //        DeletedOnUtc = table.Column<DateTime>(type: "datetime2(6)", precision: 6, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_HearAboutUs", x => x.Id);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_HearAboutUs_TenentId_Name",
            //    table: "HearAboutUs",
            //    columns: new[] { "TenentId", "Name" },
            //    unique: true,
            //    filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "HearAboutUs");
        }
    }
}
