using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Class"/> onto the physical <c>Class</c> table exactly as given — column names,
/// lengths, and the id columns' <c>nvarchar(450)</c> storage (rather than <c>uniqueidentifier</c>) are
/// preserved as-is, the same convention <c>StudentConfiguration</c> uses.
/// </summary>
internal sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Class");

        builder.HasKey(c => c.Id);

        // Id-shaped columns are stored as nvarchar(450) rather than uniqueidentifier on this table.
        builder.Property(c => c.Id).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.LocationId).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.RoomId).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.SessionId).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.PrimaryInstructorId).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.CreatedById).HasConversion<string>().HasMaxLength(450);
        builder.Property(c => c.UpdatedById).HasConversion<string>().HasMaxLength(450);

        builder.Property(c => c.CreatedOnUtc).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(c => c.Active).HasDefaultValue(true);
        builder.Property(c => c.Deleted).HasDefaultValue(false);

        builder.Property(c => c.ClassName).HasMaxLength(50);
        builder.Property(c => c.AdditionalInstructors).HasMaxLength(50);
        builder.Property(c => c.ActiveDays).HasMaxLength(200);
        builder.Property(c => c.StartTime).HasMaxLength(20);
        builder.Property(c => c.EndTime).HasMaxLength(20);
        builder.Property(c => c.Duration).HasMaxLength(50);
        builder.Property(c => c.TuitionFee).HasColumnName("TutionFee").HasPrecision(18, 2);
        builder.Property(c => c.BillingMethod).HasMaxLength(20);
        builder.Property(c => c.BillingCycle).HasMaxLength(50);
        builder.Property(c => c.Gender).HasMaxLength(30);
        builder.Property(c => c.PolicyGroups).HasMaxLength(50);
        builder.Property(c => c.VirtualClassUrl).HasColumnName("VirtualClassURL").HasMaxLength(300);
        builder.Property(c => c.LinkDisplayText).HasMaxLength(300);
        // Description is nvarchar(max) — no length cap on the live column.

        builder.Property(c => c.OnlineListings).IsRequired();
        builder.Property(c => c.OnlineRegistration).IsRequired();
        builder.Property(c => c.AllowWaitlistInRoll).IsRequired();
        builder.Property(c => c.AllowPortalEnrollment).IsRequired();
        builder.Property(c => c.AllowDropIns).IsRequired();
        builder.Property(c => c.ParentPortalSchedule).IsRequired();
        builder.Property(c => c.MakeupsInClass).IsRequired();
        builder.Property(c => c.AllowWaitlistEnrollment).IsRequired();
        builder.Property(c => c.AllowPortalDropRequests).IsRequired();
        builder.Property(c => c.DropInFee).IsRequired();
    }
}
