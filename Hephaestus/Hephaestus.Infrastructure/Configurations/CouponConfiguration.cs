using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(36);

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.DiscountType)
            .HasColumnName("discount_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.DiscountValue)
            .HasColumnName("discount_value")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(c => c.MenuItemId)
            .HasColumnName("menu_item_id")
            .HasMaxLength(36);

        builder.Property(c => c.MinOrderValue)
            .HasColumnName("min_order_value")
            .HasColumnType("decimal(10,2)");

        builder.Property(c => c.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(c => c.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(c => c.MaxTotalUses)
            .HasColumnName("max_total_uses");

        builder.Property(c => c.MaxUsesPerCustomer)
            .HasColumnName("max_uses_per_customer");

        // Índices
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.Code);
        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.MenuItemId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.StartDate);
        builder.HasIndex(c => c.EndDate);
    }
}