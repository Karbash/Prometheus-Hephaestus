using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TenantId).IsRequired().HasMaxLength(36);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(36);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Number).HasMaxLength(20);
        builder.Property(a => a.Complement).HasMaxLength(50);
        builder.Property(a => a.Neighborhood).HasMaxLength(100);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.State).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ZipCode).HasMaxLength(20);
        builder.Property(a => a.Country).HasMaxLength(100);
        builder.Property(a => a.Latitude).IsRequired();
        builder.Property(a => a.Longitude).IsRequired();
        builder.Property(a => a.Type).HasMaxLength(30);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
    }
} 