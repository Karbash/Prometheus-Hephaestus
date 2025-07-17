using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanyOperatingHourConfiguration : IEntityTypeConfiguration<CompanyOperatingHour>
{
    public void Configure(EntityTypeBuilder<CompanyOperatingHour> builder)
    {
        builder.ToTable("company_operating_hours");

        builder.HasKey(oh => oh.Id);

        builder.Property(oh => oh.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(oh => oh.CompanyId)
            .HasColumnName("company_id")
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(oh => oh.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(oh => oh.OpenTime)
            .HasColumnName("open_time")
            .HasMaxLength(5)
            .IsRequired(false);
            
        builder.Property(oh => oh.CloseTime)
            .HasColumnName("close_time")
            .HasMaxLength(5)
            .IsRequired(false);
            
        builder.Property(oh => oh.IsClosed)
            .HasColumnName("is_closed")
            .IsRequired();

        builder.HasIndex(oh => new { oh.CompanyId, oh.DayOfWeek }).IsUnique();

        // Relacionamento com Company
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(oh => oh.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}