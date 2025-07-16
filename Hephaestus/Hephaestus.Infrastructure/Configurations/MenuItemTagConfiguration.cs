using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class MenuItemTagConfiguration : IEntityTypeConfiguration<MenuItemTag>
{
    public void Configure(EntityTypeBuilder<MenuItemTag> builder)
    {
        builder.ToTable("menu_item_tags");

        builder.HasKey(mt => new { mt.MenuItemId, mt.TagId });

        builder.Property(mt => mt.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        builder.Property(mt => mt.TagId)
            .HasColumnName("tag_id")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(mt => mt.MenuItem)
            .WithMany(mi => mi.MenuItemTags)
            .HasForeignKey(mt => mt.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(mt => mt.MenuItemId);
        builder.HasIndex(mt => mt.TagId);
    }
}