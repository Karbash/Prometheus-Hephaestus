using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("coupon_usages");

        builder.HasKey(cu => cu.Id);

        builder.Property(cu => cu.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(cu => cu.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cu => cu.CouponId)
            .HasColumnName("coupon_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cu => cu.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cu => cu.OrderId)
            .HasColumnName("order_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cu => cu.UsedAt)
            .HasColumnName("used_at")
            .IsRequired();

        builder.Property(cu => cu.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cu => cu.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(cu => cu.CouponId);
        builder.HasIndex(cu => cu.CustomerId);
        builder.HasIndex(cu => cu.OrderId);
        builder.HasIndex(cu => cu.TenantId);
        builder.HasIndex(cu => cu.UsedAt);
        builder.HasIndex(cu => new { cu.CouponId, cu.CustomerId });
        builder.HasIndex(cu => new { cu.CustomerId, cu.UsedAt });
    }
} 