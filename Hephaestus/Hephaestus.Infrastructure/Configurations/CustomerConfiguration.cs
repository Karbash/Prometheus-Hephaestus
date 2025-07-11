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

        builder.Property(c => c.TenantId).IsRequired().HasMaxLength(36);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.State).IsRequired().HasMaxLength(50); // Novo campo obrigatório
        builder.Property(c => c.City).IsRequired(false).HasMaxLength(100);
        builder.Property(c => c.Street).IsRequired(false).HasMaxLength(200);
        builder.Property(c => c.Number).IsRequired(false).HasMaxLength(20);
        builder.Property(c => c.Latitude).IsRequired(false);
        builder.Property(c => c.Longitude).IsRequired(false);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.PhoneNumber }).IsUnique();
    }
}