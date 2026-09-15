using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

internal sealed class TenantPersonMappingConfiguration : IEntityTypeConfiguration<TenantPersonMapping>
{
    public void Configure(EntityTypeBuilder<TenantPersonMapping> builder)
    {
        builder.ToTable("TenantPersonMapping");

        builder.HasKey(m => m.Id);

        // A person may hold several of these (one per tenant); only an exact duplicate is blocked.
        builder.HasIndex(m => new { m.PersonId, m.TenantId }).IsUnique().HasFilter("[Deleted] = 0");
        builder.HasIndex(m => m.TenantId);

        // Restrict so a tenant cannot be removed out from under the persons assigned to it.
        builder.HasOne(m => m.Tenant)
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade so purging a person (hard delete) takes its tenant assignments with it.
        builder.HasOne(m => m.Person)
            .WithMany(p => p.TenantMappings)
            .HasForeignKey(m => m.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
