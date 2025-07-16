using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
            .HasColumnName("id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(oi => oi.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(oi => oi.OrderId)
            .HasColumnName("order_id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(oi => oi.MenuItemId)
            .HasColumnName("menu_item_id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(oi => oi.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(oi => oi.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(oi => oi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(oi => oi.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(oi => oi.TenantId);
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.MenuItemId);
    }
}