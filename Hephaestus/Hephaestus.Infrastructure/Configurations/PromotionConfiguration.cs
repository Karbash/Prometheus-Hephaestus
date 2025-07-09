using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hephaestus.Infrastructure.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.DiscountType).IsRequired().HasConversion<string>();
        builder.Property(p => p.DiscountValue).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.MenuItemId);
        builder.Property(p => p.MinOrderValue).HasPrecision(18, 2);
        builder.Property(p => p.MaxUsesPerCustomer);
        builder.Property(p => p.MaxTotalUses);
        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.EndDate).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        var listComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(p => p.ApplicableTags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(listComparer);

        builder.HasIndex(p => new { p.TenantId, p.IsActive });
    }
}
