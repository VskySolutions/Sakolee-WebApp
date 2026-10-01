using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="EPaymentSchedule"/> onto the physical
/// <c>EPaymentSchedule</c> table.
/// </summary>
internal sealed class EPaymentScheduleConfiguration : IEntityTypeConfiguration<EPaymentSchedule>
{
    #region Configure

    /// <summary>
    /// Configures the database mapping for E-Payment Schedule records.
    /// </summary>
    public void Configure(EntityTypeBuilder<EPaymentSchedule> builder)
    {
        builder.ToTable("EPaymentSchedule");
        builder.HasKey(x => x.Id);
        // Id is stored as nvarchar(450) in the existing database table.
        builder.Property(x => x.Id).HasConversion<string>().HasMaxLength(450);
        // The physical column is named "TenentId".
        builder.Property(x => x.TenantId).HasConversion<string>().HasMaxLength(450).HasColumnName("TenentId").IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Active).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(x => x.UpdatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(x => x.CreatedOnUtc).HasPrecision(6).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(x => x.UpdatedOnUtc).HasPrecision(6);
        builder.Property(x => x.DeletedOnUtc).HasPrecision(6);
        builder.Property(x => x.Deleted).HasDefaultValue(false);
        // A tenant cannot have duplicate active E-Payment Schedule names.
        builder.HasIndex(x => new { x.TenantId,x.Name }).IsUnique().HasFilter("[Deleted] = 0");
        // Tenant is populated manually by the repository.
        // TenentId is nvarchar(450), while Tenants.Id is uniqueidentifier,
        // so a physical foreign key is not configured here.
        builder.Ignore(x => x.Tenant);
    }

    #endregion
}
