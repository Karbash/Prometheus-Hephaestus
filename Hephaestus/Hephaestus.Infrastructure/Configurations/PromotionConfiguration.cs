using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(p => p.TenantId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.DiscountType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.MenuItemId)
            .HasMaxLength(36);

        builder.Property(p => p.MinOrderValue)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.MaxUsesPerCustomer);

        builder.Property(p => p.MaxTotalUses);

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        var listComparer = new ValueComparer<List<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c == null ? new List<string>() : c.ToList());

        builder.Property(p => p.ApplicableTags)
            .HasConversion(v => string.Join(',', v), v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(listComparer);

        builder.HasIndex(p => new { p.TenantId, p.IsActive });

        // Relacionamento opcional com MenuItem
        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(p => p.MenuItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
