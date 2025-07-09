using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class SalesLogConfiguration : IEntityTypeConfiguration<SalesLog>
{
    public void Configure(EntityTypeBuilder<SalesLog> builder)
    {
        builder.ToTable("SalesLogs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.CustomerPhoneNumber).IsRequired();
        builder.Property(s => s.OrderId).IsRequired();
        builder.Property(s => s.TotalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.PlatformFee).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.PromotionId).HasMaxLength(36);
        builder.Property(s => s.CouponId).HasMaxLength(36);
        builder.Property(s => s.PaymentStatus).IsRequired().HasConversion<string>();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.CustomerPhoneNumber });
    }
}
