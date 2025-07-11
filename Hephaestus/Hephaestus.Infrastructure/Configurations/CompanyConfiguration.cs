using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(100);
        builder.Property(c => c.PhoneNumber).IsRequired(false).HasMaxLength(20);
        builder.Property(c => c.ApiKey).IsRequired().HasMaxLength(100);
        builder.Property(c => c.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(c => c.Role)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(c => c.IsEnabled).IsRequired();
        builder.Property(c => c.FeeType)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(c => c.FeeValue).IsRequired().HasPrecision(18, 2);
        builder.Property(c => c.MfaSecret).IsRequired(false).HasMaxLength(100);
        builder.Property(c => c.State).IsRequired().HasMaxLength(50);
        builder.Property(c => c.City).IsRequired(false).HasMaxLength(100);
        builder.Property(c => c.Street).IsRequired(false).HasMaxLength(200);
        builder.Property(c => c.Number).IsRequired(false).HasMaxLength(20);
        builder.Property(c => c.Latitude).IsRequired(false);
        builder.Property(c => c.Longitude).IsRequired(false);
        builder.Property(c => c.Slogan).IsRequired(false).HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired(false).HasMaxLength(500);

        builder.HasIndex(c => c.Email).IsUnique();
    }
}