using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFamilyStatusDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Description",
            //    table: "FamilyStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "Description",
            //    table: "FamilyStatuses",
            //    type: "nvarchar(max)",
            //    nullable: true);
        }
    }
}
