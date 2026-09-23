using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="BillingCycle"/> onto the physical <c>BillingCycle</c> table.</summary>
internal sealed class BillingCycleConfiguration : IEntityTypeConfiguration<BillingCycle>
{
    public void Configure(EntityTypeBuilder<BillingCycle> builder)
    {
        builder.ToTable("BillingCycle");
        builder.HasKey(c => c.Id);
        // Id-shaped columns are stored as nvarchar(450) rather than uniqueidentifier on this table.
        builder.Property(c => c.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.TenantId).HasConversion<string>().HasMaxLength(450).HasColumnName("TenentId").IsRequired();
        builder.Property(c => c.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.UpdatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
        builder.Property(c => c.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(c => c.Deleted).HasDefaultValue(false);
        // A tenant may not have the same billing cycle name more than once.
        builder.HasIndex(c => new { c.TenantId, c.Name }).IsUnique().HasFilter("[Deleted] = 0");
        // Tenant is populated manually by the repository. TenentId is nvarchar(450)
        // while Tenants.Id is uniqueidentifier, so a real FK is not possible on this table.
        builder.Ignore(c => c.Tenant);
    }
}