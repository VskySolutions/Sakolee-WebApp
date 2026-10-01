using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AccountType"/> onto the physical
/// <c>AccountType</c> table.
/// </summary>
internal sealed class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    #region Configure

    /// <summary>
    /// Configures the database mapping for Account Type records.
    /// </summary>
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.ToTable("AccountType");
        builder.HasKey(x => x.Id);
        // Id is stored as nvarchar(450).
        builder.Property(x => x.Id).HasConversion<string>().HasMaxLength(450);
        // Physical database column is "TenentId".
        builder.Property(x => x.TenantId).HasConversion<string>().HasMaxLength(450).HasColumnName("TenentId").IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Active).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(x => x.UpdatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(x => x.CreatedOnUtc).HasPrecision(6).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(x => x.UpdatedOnUtc).HasPrecision(6);
        builder.Property(x => x.DeletedOnUtc).HasPrecision(6);
        builder.Property(x => x.Deleted).HasDefaultValue(false);
        // A tenant cannot have duplicate non-deleted Account Type names.
        builder.HasIndex(x => new{ x.TenantId,x.Name}).IsUnique().HasFilter("[Deleted] = 0");
        // Tenant is populated manually by the repository.
        // No physical FK is configured because the existing
        // TenentId column is nvarchar while Tenant.Id is Guid.
        builder.Ignore(x => x.Tenant);
    }

    #endregion
}