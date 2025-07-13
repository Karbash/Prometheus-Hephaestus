using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class AdditionalConfiguration : IEntityTypeConfiguration<Additional>
{
    public void Configure(EntityTypeBuilder<Additional> builder)
    {
        builder.ToTable("Additionals");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(36); // GUID como string

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(a => new { a.TenantId, a.Name })
            .IsUnique(); // Garante que nomes de adicionais sejam únicos por tenant
    }
}