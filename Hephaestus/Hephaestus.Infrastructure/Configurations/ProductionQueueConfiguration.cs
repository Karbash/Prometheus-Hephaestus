﻿using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class ProductionQueueConfiguration : IEntityTypeConfiguration<ProductionQueue>
{
    public void Configure(EntityTypeBuilder<ProductionQueue> builder)
    {
        builder.ToTable("production_queues");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.Priority).IsRequired();
        builder.Property(p => p.Status).IsRequired().HasConversion<string>();

        builder.HasIndex(p => p.OrderId);

        // Relacionamento com Order
        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
