using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class MenuItemAdditionalConfiguration : IEntityTypeConfiguration<MenuItemAdditional>
{
    public void Configure(EntityTypeBuilder<MenuItemAdditional> builder)
    {
        builder.ToTable("menu_item_additionals");

        builder.HasKey(ma => new { ma.MenuItemId, ma.AdditionalId });

        builder.Property(ma => ma.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        builder.Property(ma => ma.AdditionalId)
            .HasColumnName("additional_id")
            .IsRequired();

        builder.Property(ma => ma.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(ma => ma.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ma => ma.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(ma => ma.MenuItem)
            .WithMany(mi => mi.MenuItemAdditionals)
            .HasForeignKey(ma => ma.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ma => ma.Additional)
            .WithMany()
            .HasForeignKey(ma => ma.AdditionalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(ma => ma.MenuItemId);
        builder.HasIndex(ma => ma.AdditionalId);
    }
} 