using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(mi => mi.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(mi => mi.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mi => mi.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(mi => mi.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(mi => mi.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(mi => mi.IsAvailable)
            .HasColumnName("is_available")
            .IsRequired();

        builder.Property(mi => mi.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);

        builder.Property(mi => mi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(mi => mi.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(mi => mi.MenuItemTags)
            .WithOne()
            .HasForeignKey(mt => mt.MenuItemId);

        builder.HasMany(mi => mi.MenuItemAdditionals)
            .WithOne()
            .HasForeignKey(ma => ma.MenuItemId);

        // Índices
        builder.HasIndex(mi => mi.TenantId);
        builder.HasIndex(mi => mi.CategoryId);
        builder.HasIndex(mi => mi.IsAvailable);
        builder.HasIndex(mi => mi.CreatedAt);
    }
}