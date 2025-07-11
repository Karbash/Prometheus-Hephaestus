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
        builder.Property(c => c.Name).IsRequired();
        builder.Property(c => c.Email).IsRequired();
        builder.Property(c => c.PhoneNumber).IsRequired(false);
        builder.Property(c => c.ApiKey).IsRequired();
        builder.Property(c => c.PasswordHash).IsRequired();
        builder.Property(c => c.Role)
            .IsRequired()
            .HasConversion<string>(); // Mapeia o enum Role como string
        builder.Property(c => c.IsEnabled).IsRequired();
        builder.Property(c => c.FeeType)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(c => c.FeeValue).IsRequired();
        builder.Property(c => c.MfaSecret).IsRequired(false);
        builder.Property(c => c.City).IsRequired(false);
        builder.Property(c => c.Street).IsRequired(false);
        builder.Property(c => c.Number).IsRequired(false);
        builder.Property(c => c.Latitude).IsRequired(false);
        builder.Property(c => c.Longitude).IsRequired(false);
        builder.Property(c => c.Slogan).IsRequired(false);
        builder.Property(c => c.Description).IsRequired(false);

        builder.HasIndex(c => c.Email).IsUnique();
    }
}