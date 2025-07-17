using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanySocialMediaConfiguration : IEntityTypeConfiguration<CompanySocialMedia>
{
    public void Configure(EntityTypeBuilder<CompanySocialMedia> builder)
    {
        builder.ToTable("company_social_media");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(sm => sm.CompanyId)
            .HasColumnName("company_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(sm => sm.Platform)
            .HasColumnName("platform")
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(sm => sm.Url)
            .HasColumnName("url")
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(sm => new { sm.CompanyId, sm.Platform }).IsUnique();

        // Relacionamento com Company
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(sm => sm.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}