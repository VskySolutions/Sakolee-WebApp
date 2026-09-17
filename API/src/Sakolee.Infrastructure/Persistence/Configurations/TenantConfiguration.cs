using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sakolee.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Identifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        // Tenant identifiers must be unique, URL-safe slugs.
        builder.HasIndex(t => t.Identifier).IsUnique().HasFilter("[Deleted] = 0");

        // The tenant's own address and default-administrator Person (no cascade — both are independently
        // owned records; a tenant losing its address/admin reference must not take either down with it).
        builder.HasOne(t => t.Address)
            .WithMany()
            .HasForeignKey(t => t.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Person)
            .WithMany()
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
