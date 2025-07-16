using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class MenuItemImageConfiguration : IEntityTypeConfiguration<MenuItemImage>
{
    public void Configure(EntityTypeBuilder<MenuItemImage> builder)
    {
        builder.ToTable("MenuItemImages");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.MenuItemId).IsRequired().HasMaxLength(36);
        builder.Property(mi => mi.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(mi => mi.CreatedAt).IsRequired();

        builder.HasIndex(mi => mi.MenuItemId);

        // Relacionamento com MenuItem
        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(mi => mi.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
