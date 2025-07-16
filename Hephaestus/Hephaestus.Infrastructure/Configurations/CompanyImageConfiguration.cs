using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanyImageConfiguration : IEntityTypeConfiguration<CompanyImage>
{
    public void Configure(EntityTypeBuilder<CompanyImage> builder)
    {
        builder.ToTable("CompanyImages");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CompanyId).IsRequired().HasMaxLength(36);
        builder.Property(ci => ci.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(ci => ci.ImageType).IsRequired().HasMaxLength(50);
        builder.Property(ci => ci.CreatedAt).IsRequired();

        builder.HasIndex(ci => new { ci.CompanyId, ci.ImageType });

        // Relacionamento com Company
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(ci => ci.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
