using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(o => o.CustomerPhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.PlatformFee)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.PromotionId)
            .HasMaxLength(36);

        builder.Property(o => o.CouponId)
            .HasMaxLength(36);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        builder.HasIndex(o => new { o.TenantId, o.CustomerPhoneNumber });

        // Relacionamentos opcionais com Promotion e Coupon
        builder.HasOne<Promotion>()
            .WithMany()
            .HasForeignKey(o => o.PromotionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
