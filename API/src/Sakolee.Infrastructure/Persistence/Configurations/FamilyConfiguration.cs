using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Family"/> onto the physical <c>Families</c> table (renamed from the legacy
/// <c>Parents</c> table by <c>RenameParentsAndParentContactsToFamilies</c>) — column names, lengths,
/// and the legacy id columns' <c>nvarchar(450)</c> storage (rather than <c>uniqueidentifier</c>) are
/// preserved as-is from that table, the same convention <c>StudentConfiguration</c>/
/// <c>PersonConfiguration</c> use for their own legacy-shaped tables. Columns added later by
/// <c>ExtendParentsAndParentContactsForFamilies</c> use proper native types, since those are
/// greenfield additions rather than inherited legacy shape.
/// </summary>
internal sealed class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("Families");

        builder.HasKey(f => f.Id);

        // Id-shaped columns are stored as nvarchar(450) rather than uniqueidentifier on this table —
        // same convention as Student/Person.
        builder.Property(f => f.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(f => f.PersonId).HasConversion<string>().HasMaxLength(450);
        builder.Property(f => f.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(f => f.UpdatedById).HasConversion<string>().HasMaxLength(450);

        builder.Property(f => f.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(f => f.Active).HasDefaultValue(true);
        builder.Property(f => f.Deleted).HasDefaultValue(false);

        builder.Property(f => f.FamilyName).HasMaxLength(100);
        builder.Property(f => f.FirstName).HasMaxLength(100);
        builder.Property(f => f.LastName).HasMaxLength(100);
        builder.Property(f => f.Type).HasMaxLength(100);
        builder.Property(f => f.Email).HasMaxLength(100);
        builder.Property(f => f.HomePhone).HasMaxLength(100);
        builder.Property(f => f.WorkPhone).HasMaxLength(100);
        builder.Property(f => f.CellPhone).HasMaxLength(100);
        builder.Property(f => f.Fax).HasMaxLength(100);
        builder.Property(f => f.OtherPhone).HasMaxLength(100);
        builder.Property(f => f.City).HasMaxLength(100);
        builder.Property(f => f.State).HasMaxLength(100);
        // Address1/Address2/MassEmailOptOut/TextOptIn are nvarchar(max) — no length cap on the live column.

        // ---- Columns added by ExtendParentsAndParentContactsForFamilies ----
        builder.Property(f => f.Source).HasMaxLength(20);
        builder.Property(f => f.ReferralName).HasMaxLength(50);
        builder.Property(f => f.EmergencyContactPerson).HasMaxLength(150);
        builder.Property(f => f.EmergencyPhone).HasMaxLength(30);
        builder.Property(f => f.HealthInsuranceCarrier).HasMaxLength(150);

        builder.HasIndex(f => f.TenantId);
        builder.HasIndex(f => new { f.TenantId, f.FamilyName });

        builder.HasOne(f => f.Tenant)
            .WithMany()
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.StudioLocation)
            .WithMany()
            .HasForeignKey(f => f.StudioLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.FamilyStatus)
            .WithMany()
            .HasForeignKey(f => f.FamilyStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
