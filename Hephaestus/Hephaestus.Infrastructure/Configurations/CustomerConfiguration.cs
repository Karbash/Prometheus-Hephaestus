using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Latitude);
        builder.Property(c => c.Longitude);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.PhoneNumber }).IsUnique();
    }
}
