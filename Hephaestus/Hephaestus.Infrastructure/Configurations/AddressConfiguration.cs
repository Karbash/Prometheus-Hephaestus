using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .IsRequired()
            .HasMaxLength(30);
            
        builder.Property(a => a.Street)
            .HasColumnName("street")
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(a => a.Number)
            .HasColumnName("number")
            .HasMaxLength(20);
            
        builder.Property(a => a.Complement)
            .HasColumnName("complement")
            .HasMaxLength(50);
            
        builder.Property(a => a.Neighborhood)
            .HasColumnName("neighborhood")
            .HasMaxLength(100);
            
        builder.Property(a => a.City)
            .HasColumnName("city")
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(a => a.State)
            .HasColumnName("state")
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(a => a.ZipCode)
            .HasColumnName("zip_code")
            .HasMaxLength(20);
            
        builder.Property(a => a.Country)
            .HasColumnName("country")
            .HasMaxLength(100);
            
        builder.Property(a => a.Latitude)
            .HasColumnName("latitude")
            .IsRequired();
            
        builder.Property(a => a.Longitude)
            .HasColumnName("longitude")
            .IsRequired();
            
        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasMaxLength(30);
            
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
            
        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
} 