using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="FamilyPersonMapping"/> onto the physical <c>FamilyPersonMapping</c> table (renamed
/// from <c>FamilyContacts</c>, itself renamed from the legacy <c>ParentContacts</c> table by
/// <c>RenameParentsAndParentContactsToFamilies</c>) — see <c>FamilyConfiguration</c>'s remarks for the
/// same convention applied to <c>Families</c>.
/// </summary>
internal sealed class FamilyPersonMappingConfiguration : IEntityTypeConfiguration<FamilyPersonMapping>
{
    public void Configure(EntityTypeBuilder<FamilyPersonMapping> builder)
    {
        builder.ToTable("FamilyPersonMapping");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.FamilyId).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.ContactTypeId).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.UpdatedById).HasConversion<string>().HasMaxLength(450);
        // PersonId is a plain uniqueidentifier — added fresh by ExtendParentsAndParentContactsForFamilies,
        // unlike the legacy nvarchar(450) id columns above.

        builder.Property(c => c.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(c => c.Active).HasDefaultValue(true);
        builder.Property(c => c.Deleted).HasDefaultValue(false);

        builder.Property(c => c.ContactName).HasMaxLength(400);
        builder.Property(c => c.EmailAddress).HasMaxLength(600);
        builder.Property(c => c.PhoneNumber).HasMaxLength(60);

        // ---- Added by ExtendParentsAndParentContactsForFamilies ----
        builder.Property(c => c.Relation).HasMaxLength(50);

        // ---- Added by AddIsPrimaryContactToFamilyPersonMapping ----
        builder.Property(c => c.IsPrimaryContact).HasDefaultValue(false);

        builder.HasIndex(c => c.FamilyId);

        // The physical FK (renamed FK_FamilyPersonMapping_Families_FamilyId, from
        // FK_FamilyPersonMapping_Families_ParentId, from FK_FamilyContacts_Families_ParentId, from
        // FK_ParentContacts_Parents) is NO ACTION, not CASCADE — matched here so EF doesn't see a
        // model/DB mismatch to "fix" later.
        builder.HasOne(c => c.Family)
            .WithMany(f => f.Contacts)
            .HasForeignKey(c => c.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
