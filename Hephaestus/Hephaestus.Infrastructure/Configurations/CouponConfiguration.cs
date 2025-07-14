using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CustomerPhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.DiscountType)
            .IsRequired()
            .HasConversion<string>(); // Garante que DiscountType seja armazenado como string

        builder.Property(c => c.DiscountValue)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.MenuItemId)
            .HasMaxLength(36);

        builder.Property(c => c.MinOrderValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique();
    }
}