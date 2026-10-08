using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Policy"/> onto the physical <c>Policy</c> table.</summary>
internal sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    #region Configure

    /// <summary>
    /// Configures the database mapping for Policy records.
    /// </summary>
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policy");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        // Content is nvarchar(max): the full policy text as HTML.
        builder.Property(p => p.Content);
        builder.Property(p => p.Active).IsRequired().HasDefaultValue(true);
        builder.Property(p => p.DisplayOrder).HasDefaultValue(0);
        builder.Property(p => p.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(p => p.Deleted).HasDefaultValue(false);

        // A tenant cannot have two live policies with the same name.
        builder.HasIndex(p => new { p.TenantId, p.Name }).IsUnique().HasFilter("[Deleted] = 0");

        // One policy → many class mappings, through the FK_PolicyClass_Policy constraint.
        builder.HasMany(p => p.ClassMappings)
            .WithOne(m => m.Policy)
            .HasForeignKey(m => m.PolicyId)
            .HasConstraintName("FK_PolicyClass_Policy");
    }

    #endregion
}

/// <summary>Maps <see cref="PolicyClassMapping"/> onto the physical <c>PolicyClassMapping</c> table.</summary>
internal sealed class PolicyClassMappingConfiguration : IEntityTypeConfiguration<PolicyClassMapping>
{
    #region Configure

    /// <summary>
    /// Configures the database mapping for policy-to-class join rows.
    /// </summary>
    public void Configure(EntityTypeBuilder<PolicyClassMapping> builder)
    {
        builder.ToTable("PolicyClassMapping");

        // A class is linked to a policy at most once.
        builder.HasKey(m => new { m.PolicyId, m.ClassId });

        // Supports the class side lookup (a class's policies).
        builder.HasIndex(m => m.ClassId);
    }

    #endregion
}
