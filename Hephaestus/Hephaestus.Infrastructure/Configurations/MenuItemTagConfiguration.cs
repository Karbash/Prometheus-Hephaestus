using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class MenuItemTagConfiguration : IEntityTypeConfiguration<MenuItemTag>
{
    public void Configure(EntityTypeBuilder<MenuItemTag> builder)
    {
        builder.ToTable("MenuItemTags");

        builder.HasKey(mt => new { mt.MenuItemId, mt.TagId });

        builder.HasOne(mt => mt.MenuItem)
            .WithMany(m => m.MenuItemTags)
            .HasForeignKey(mt => mt.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mt => mt.Tag)
            .WithMany()
            .HasForeignKey(mt => mt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
