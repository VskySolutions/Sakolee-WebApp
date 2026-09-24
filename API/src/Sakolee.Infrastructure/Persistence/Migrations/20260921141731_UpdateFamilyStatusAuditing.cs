using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakolee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFamilyStatusAuditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty. This migration renamed FamilyStatuses columns (IsDeleted -> IsActive,
            // CreatedBy -> Code, UpdatedBy -> Description, ...) to names the FamilyStatus entity has never
            // used, which broke Family Statuses on every database it ran on. FamilyStatuses keeps its
            // original columns; the migration stays only so the history is unchanged.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — see Up.
        }
    }
}
