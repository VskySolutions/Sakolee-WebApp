using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Student"/> onto the physical <c>Students</c> table exactly as it exists in the
/// database (managed outside this project's migration history) — column names, lengths, and even the
/// id columns' odd <c>nvarchar(450)</c> storage (rather than <c>uniqueidentifier</c>) are preserved
/// as-is rather than corrected, so this configuration never drifts from what is actually there.
/// </summary>
internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        // Id-shaped columns are stored as nvarchar(450) rather than uniqueidentifier on this table.
        builder.Property(s => s.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(s => s.ParentId).HasConversion<string>().HasMaxLength(450);
        builder.Property(s => s.PersonId).HasConversion<string>().HasMaxLength(450);
        builder.Property(s => s.ClassId).HasConversion<string>().HasMaxLength(450);
        builder.Property(s => s.FeeCategoryId).HasConversion<string>().HasMaxLength(450);
        builder.Property(s => s.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(s => s.UpdatedById).HasConversion<string>().HasMaxLength(450);

        builder.Property(s => s.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(s => s.Active).HasDefaultValue(true);
        builder.Property(s => s.Deleted).HasDefaultValue(false);

        builder.Property(s => s.StudentNumber).HasMaxLength(100);
        builder.Property(s => s.AdmissionDate).HasColumnType("date");
        builder.Property(s => s.FeeAmount).HasPrecision(18, 2);
        builder.Property(s => s.FeeExpiryDate).HasColumnName("FeeExcpirydate");
        builder.Property(s => s.FeeNote).HasMaxLength(300);
        builder.Property(s => s.FamilyName).HasMaxLength(50);
        builder.Property(s => s.CellPhone).HasMaxLength(50);
        builder.Property(s => s.School).HasMaxLength(100);
        builder.Property(s => s.GradeLevel).HasMaxLength(50);
        builder.Property(s => s.Transportation).HasMaxLength(100);
        builder.Property(s => s.TShirtSize).HasMaxLength(10);
        builder.Property(s => s.SpecialNeeds).HasMaxLength(50);
        builder.Property(s => s.PrimaryDoctor).HasMaxLength(50);
        // Medications, ImmunizationNotes, SkillNotes, TextOptIn, MassEmailOptOut are nvarchar(max) —
        // no length cap on the live column.
    }
}
