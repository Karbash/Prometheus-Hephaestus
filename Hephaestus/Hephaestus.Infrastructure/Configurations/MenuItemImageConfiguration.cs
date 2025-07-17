using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class MenuItemImageConfiguration : IEntityTypeConfiguration<MenuItemImage>
{
    public void Configure(EntityTypeBuilder<MenuItemImage> builder)
    {
        builder.ToTable("menu_item_images");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(mi => mi.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(mi => mi.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();
            
        builder.Property(mi => mi.ImageUrl)
            .HasColumnName("image_url")
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(mi => mi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
            
        builder.Property(mi => mi.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}