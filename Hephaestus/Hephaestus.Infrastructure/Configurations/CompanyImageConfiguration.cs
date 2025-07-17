using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanyImageConfiguration : IEntityTypeConfiguration<CompanyImage>
{
    public void Configure(EntityTypeBuilder<CompanyImage> builder)
    {
        builder.ToTable("company_images");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ci => ci.CompanyId)
            .HasColumnName("company_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(ci => ci.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(ci => ci.ImageUrl)
            .HasColumnName("image_url")
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(ci => ci.ImageType)
            .HasColumnName("image_type")
            .HasMaxLength(50);
            
        builder.Property(ci => ci.IsMain)
            .HasColumnName("is_main")
            .IsRequired();
            
        builder.Property(ci => ci.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
            
        builder.Property(ci => ci.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}