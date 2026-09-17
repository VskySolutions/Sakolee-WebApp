using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LocationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RoomId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PrimaryInstructorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdditionalInstructors = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationOpenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActiveDays = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TutionFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BillingMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BillingCycle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationFee = table.Column<bool>(type: "bit", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MinAge = table.Column<int>(type: "int", nullable: true),
                    MaxAge = table.Column<int>(type: "int", nullable: true),
                    MaxClassSize = table.Column<int>(type: "int", nullable: true),
                    MaxWaitlistSize = table.Column<int>(type: "int", nullable: true),
                    CutoffDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PolicyGroups = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VirtualClassURL = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LinkDisplayText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    OnlineListings = table.Column<bool>(type: "bit", nullable: false),
                    OnlineRegistration = table.Column<bool>(type: "bit", nullable: false),
                    AllowWaitlistInRoll = table.Column<bool>(type: "bit", nullable: false),
                    AllowPortalEnrollment = table.Column<bool>(type: "bit", nullable: false),
                    AllowDropIns = table.Column<bool>(type: "bit", nullable: false),
                    ParentPortalSchedule = table.Column<bool>(type: "bit", nullable: false),
                    MakeupsInClass = table.Column<bool>(type: "bit", nullable: false),
                    AllowWaitlistEnrollment = table.Column<bool>(type: "bit", nullable: false),
                    AllowPortalDropRequests = table.Column<bool>(type: "bit", nullable: false),
                    DropInFee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Class");
        }
    }
}
