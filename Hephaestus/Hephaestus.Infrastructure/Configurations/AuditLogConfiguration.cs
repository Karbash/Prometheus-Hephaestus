using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.TenantId).IsRequired(false);
        builder.Property(a => a.UserId).IsRequired(false); 
        builder.Property(a => a.Action).IsRequired();
        builder.Property(a => a.EntityId).IsRequired();
        builder.Property(a => a.Details).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.HasIndex(a => a.UserId).IsUnique(false);
        builder.HasIndex(a => a.TenantId).IsUnique(false); 
    }
}