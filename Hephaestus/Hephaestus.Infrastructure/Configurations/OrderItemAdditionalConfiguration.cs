using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class OrderItemAdditionalConfiguration : IEntityTypeConfiguration<OrderItemAdditional>
{
    public void Configure(EntityTypeBuilder<OrderItemAdditional> builder)
    {
        builder.ToTable("orderitemadditional");
        builder.HasKey(x => new { x.OrderItemId, x.AdditionalId });

        builder.Property(x => x.OrderItemId)
            .HasColumnName("orderitemid")
            .IsRequired();

        builder.Property(x => x.AdditionalId)
            .HasColumnName("additionalid")
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
            .WithMany(x => x.OrderItemAdditionals)
            .HasForeignKey(x => x.OrderItemId);

        builder.HasOne(x => x.Additional)
            .WithMany()
            .HasForeignKey(x => x.AdditionalId);
    }
} 