using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

internal sealed class TenantRoleOverrideConfiguration : IEntityTypeConfiguration<TenantRoleOverride>
{
    public void Configure(EntityTypeBuilder<TenantRoleOverride> builder)
    {
        builder.ToTable("TenantRoleOverrides");

        builder.HasKey(o => o.Id);

        // Permission keys stored as a JSON array column, like Role.Permissions.
        builder.Property(o => o.Permissions)
            .HasMaxLength(4000);

        builder.HasOne(o => o.Role)
            .WithMany(r => r.TenantOverrides)
            .HasForeignKey(o => o.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // A tenant has at most one live permission set per role.
        builder.HasIndex(o => new { o.TenantId, o.RoleId }).IsUnique().HasFilter("[Deleted] = 0");
    }
}
