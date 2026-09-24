using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="TShirtSize"/> onto the physical <c>TShirtSize</c> table.
/// </summary>
internal sealed class TShirtSizeConfiguration : IEntityTypeConfiguration<TShirtSize>
{
    /// <summary>
    /// Configures the T-Shirt Size entity, including its table mapping,
    /// properties, relationships, default values, and unique constraints.
    /// </summary>
    public void Configure(EntityTypeBuilder<TShirtSize> builder)
    {
        // Map the entity to the TShirtSize database table.
        builder.ToTable("TShirtSize");
        // Configure the primary key for the T-Shirt Size entity.
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.TenantId).HasConversion<string>().HasMaxLength(450).HasColumnName("TenentId").IsRequired();
        builder.Property(c => c.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.UpdatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
        builder.Property(c => c.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(c => c.Deleted).HasDefaultValue(false);
        // Create a unique filtered index on TenantId and Name.
        // This prevents duplicate active T-Shirt Size names within the same tenant
        // while allowing previously soft-deleted records to have the same name.
        builder.HasIndex(c => new { c.TenantId,c.Name }).IsUnique().HasFilter("[Deleted] = 0");
        // Ignore the Tenant navigation property because it is not mapped as a relationship in this configuration.
        builder.Ignore(c => c.Tenant);
    }
}