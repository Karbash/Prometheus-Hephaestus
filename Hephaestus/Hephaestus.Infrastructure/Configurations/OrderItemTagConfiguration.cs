using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class OrderItemTagConfiguration : IEntityTypeConfiguration<OrderItemTag>
{
    public void Configure(EntityTypeBuilder<OrderItemTag> builder)
    {
        builder.ToTable("orderitemtag");
        builder.HasKey(x => new { x.OrderItemId, x.TagId });

        builder.Property(x => x.OrderItemId)
            .HasColumnName("orderitemid")
            .IsRequired();

        builder.Property(x => x.TagId)
            .HasColumnName("tagid")
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenantid")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("createdat")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updatedat")
            .IsRequired();

        builder.HasOne(x => x.OrderItem)
            .WithMany(x => x.OrderItemTags)
            .HasForeignKey(x => x.OrderItemId);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId);
    }
} 