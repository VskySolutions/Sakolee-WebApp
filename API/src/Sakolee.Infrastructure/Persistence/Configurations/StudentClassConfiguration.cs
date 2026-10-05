using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

internal sealed class StudentClassConfiguration : IEntityTypeConfiguration<StudentClass>
{
    public void Configure(EntityTypeBuilder<StudentClass> builder)
    {
        builder.ToTable("StudentClasses");

        builder.HasKey(sc => sc.Id);

        builder.HasIndex(sc => sc.ClassId);

        // A student is enrolled in a class at most once.
        builder.HasIndex(sc => new { sc.StudentId, sc.ClassId }).IsUnique().HasFilter("[Deleted] = 0");
    }
}
