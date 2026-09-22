using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="ClassCategory"/> onto the physical <c>ClassCategory</c> table.</summary>
internal sealed class ClassCategoryConfiguration : IEntityTypeConfiguration<ClassCategory>
{
    public void Configure(EntityTypeBuilder<ClassCategory> builder)
    {
        builder.ToTable("ClassCategory");

        builder.HasKey(c => c.Id);

        // Id-shaped columns are stored as nvarchar(450) rather than uniqueidentifier on this table.
        builder.Property(c => c.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.TenantId).HasConversion<string>().HasMaxLength(450).HasColumnName("TenentId").IsRequired();
        builder.Property(c => c.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.UpdatedById).HasConversion<string>().HasMaxLength(450);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CategoryType).HasMaxLength(30);

        builder.Property(c => c.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(c => c.Deleted).HasDefaultValue(false);

        // A tenant may not list the same option twice under the same dropdown.
        builder.HasIndex(c => new { c.TenantId, c.CategoryType, c.Name }).IsUnique().HasFilter("[Deleted] = 0");
        // Tenant is populated manually by the repository. TenentId is nvarchar(450) while Tenants.Id is
        // uniqueidentifier, so a real FK is not possible on this table.
        builder.Ignore(c => c.Tenant);
    }
}
