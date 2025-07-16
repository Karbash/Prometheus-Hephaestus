using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(al => al.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(al => al.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(al => al.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(al => al.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(50);

        builder.Property(al => al.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(50);

        builder.Property(al => al.CompanyId)
            .HasColumnName("company_id")
            .HasMaxLength(50);

        builder.Property(al => al.Details)
            .HasColumnName("details")
            .HasMaxLength(4000);

        builder.Property(al => al.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(al => al.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(al => al.CompanyId);
        builder.HasIndex(al => al.Action);
        builder.HasIndex(al => al.EntityType);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.TenantId);
        builder.HasIndex(al => al.CreatedAt);
        builder.HasIndex(al => new { al.CompanyId, al.CreatedAt });
        builder.HasIndex(al => new { al.CompanyId, al.Action });
        builder.HasIndex(al => new { al.CompanyId, al.EntityType });
        builder.HasIndex(al => new { al.EntityType, al.EntityId });
        builder.HasIndex(al => new { al.UserId, al.CreatedAt });
    }
}