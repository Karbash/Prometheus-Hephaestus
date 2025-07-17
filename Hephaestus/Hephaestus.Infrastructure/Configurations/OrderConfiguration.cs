using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(o => o.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(o => o.AddressId)
            .HasColumnName("address_id")
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.PlatformFee)
            .HasColumnName("platform_fee")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.PromotionId)
            .HasColumnName("promotion_id");

        builder.Property(o => o.CouponId)
            .HasColumnName("coupon_id");

        builder.Property(o => o.DeliveryType)
            .HasColumnName("delivery_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasColumnName("payment_status")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Removido: builder.HasMany(o => o.OrderItems).WithOne().HasForeignKey(oi => oi.OrderId);

        // Índices
        builder.HasIndex(o => o.TenantId);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.CompanyId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);
        builder.HasIndex(o => o.CreatedAt);
    }
}