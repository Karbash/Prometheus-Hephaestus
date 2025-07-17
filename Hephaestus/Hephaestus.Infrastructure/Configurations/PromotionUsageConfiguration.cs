using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class PromotionUsageConfiguration : IEntityTypeConfiguration<PromotionUsage>
{
    public void Configure(EntityTypeBuilder<PromotionUsage> builder)
    {
        builder.ToTable("promotion_usages");

        builder.HasKey(pu => pu.Id);

        builder.Property(pu => pu.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(pu => pu.CompanyId)
            .HasColumnName("company_id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(pu => pu.PromotionId)
            .HasColumnName("promotion_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pu => pu.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pu => pu.OrderId)
            .HasColumnName("order_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pu => pu.UsedAt)
            .HasColumnName("used_at")
            .IsRequired();

        builder.Property(pu => pu.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pu => pu.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(pu => pu.PromotionId);
        builder.HasIndex(pu => pu.CustomerId);
        builder.HasIndex(pu => pu.OrderId);
        builder.HasIndex(pu => pu.CompanyId);
        builder.HasIndex(pu => pu.UsedAt);
        builder.HasIndex(pu => new { pu.PromotionId, pu.CustomerId });
        builder.HasIndex(pu => new { pu.CustomerId, pu.UsedAt });
    }
} 