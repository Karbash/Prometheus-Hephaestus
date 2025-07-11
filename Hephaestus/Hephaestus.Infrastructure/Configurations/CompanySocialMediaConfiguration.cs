using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanySocialMediaConfiguration : IEntityTypeConfiguration<CompanySocialMedia>
{
    public void Configure(EntityTypeBuilder<CompanySocialMedia> builder)
    {
        builder.ToTable("CompanySocialMedia");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.CompanyId).IsRequired().HasMaxLength(36);
        builder.Property(sm => sm.Platform).IsRequired().HasMaxLength(50);
        builder.Property(sm => sm.Url).IsRequired().HasMaxLength(500);

        builder.HasIndex(sm => new { sm.CompanyId, sm.Platform }).IsUnique();
    }
}