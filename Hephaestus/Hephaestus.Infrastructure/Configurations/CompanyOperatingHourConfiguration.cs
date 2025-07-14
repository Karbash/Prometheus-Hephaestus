using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanyOperatingHourConfiguration : IEntityTypeConfiguration<CompanyOperatingHour>
{
    public void Configure(EntityTypeBuilder<CompanyOperatingHour> builder)
    {
        builder.ToTable("CompanyOperatingHours");

        builder.HasKey(oh => oh.Id);

        builder.Property(oh => oh.CompanyId).IsRequired().HasMaxLength(36);
        builder.Property(oh => oh.DayOfWeek).IsRequired().HasMaxLength(10);
        builder.Property(oh => oh.OpenTime).HasMaxLength(5).IsRequired(false);
        builder.Property(oh => oh.CloseTime).HasMaxLength(5).IsRequired(false);
        builder.Property(oh => oh.IsClosed).IsRequired();

        builder.HasIndex(oh => new { oh.CompanyId, oh.DayOfWeek }).IsUnique();

        // Relacionamento com Company
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(oh => oh.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}