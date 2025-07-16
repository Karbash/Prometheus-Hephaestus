using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.DiscountType)
            .HasColumnName("discount_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.DiscountValue)
            .HasColumnName("discount_value")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.MenuItemId)
            .HasColumnName("menu_item_id")
            .HasMaxLength(36);

        builder.Property(p => p.MinOrderValue)
            .HasColumnName("min_order_value")
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.MaxTotalUses)
            .HasColumnName("max_total_uses");

        builder.Property(p => p.MaxUsesPerCustomer)
            .HasColumnName("max_uses_per_customer");

        builder.Property(p => p.DaysOfWeek)
            .HasColumnName("days_of_week")
            .HasMaxLength(50);

        builder.Property(p => p.Hours)
            .HasColumnName("hours")
            .HasMaxLength(50);

        builder.Property(p => p.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(p => p.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.MenuItemId);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.StartDate);
        builder.HasIndex(p => p.EndDate);
    }
}