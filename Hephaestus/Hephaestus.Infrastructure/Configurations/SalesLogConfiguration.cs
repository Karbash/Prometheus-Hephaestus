using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class SalesLogConfiguration : IEntityTypeConfiguration<SalesLog>
{
    public void Configure(EntityTypeBuilder<SalesLog> builder)
    {
        builder.ToTable("sales_logs");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(sl => sl.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.CompanyId)
            .HasColumnName("company_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.OrderId)
            .HasColumnName("order_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(sl => sl.PlatformFee)
            .HasColumnName("platform_fee")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(sl => sl.PromotionId)
            .HasColumnName("promotion_id")
            .HasMaxLength(50);

        builder.Property(sl => sl.CouponId)
            .HasColumnName("coupon_id")
            .HasMaxLength(50);

        builder.Property(sl => sl.PaymentStatus)
            .HasColumnName("payment_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sl => sl.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();



        // Índices
        builder.HasIndex(sl => sl.CompanyId);
        builder.HasIndex(sl => sl.OrderId);
        builder.HasIndex(sl => sl.CustomerId);
        builder.HasIndex(sl => sl.TenantId);
        builder.HasIndex(sl => sl.CreatedAt);
        builder.HasIndex(sl => sl.PaymentStatus);
        builder.HasIndex(sl => new { sl.CompanyId, sl.CreatedAt });
        builder.HasIndex(sl => new { sl.CompanyId, sl.PaymentStatus });
        builder.HasIndex(sl => new { sl.CustomerId, sl.CreatedAt });
        builder.HasIndex(sl => new { sl.CreatedAt, sl.PaymentStatus });
    }
}
