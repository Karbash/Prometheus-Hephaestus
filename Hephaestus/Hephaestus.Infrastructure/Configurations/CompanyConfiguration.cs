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

        builder.Property(c => c.Id).HasMaxLength(36).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);
        builder.Property(c => c.ApiKey).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Role).HasConversion<string>().IsRequired();
        builder.Property(c => c.IsEnabled).IsRequired();
        builder.Property(c => c.FeeType).HasConversion<string>().IsRequired();
        builder.Property(c => c.FeeValue).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(c => c.MfaSecret).HasMaxLength(100);
        builder.Property(c => c.State).HasMaxLength(100).IsRequired();
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Neighborhood).HasMaxLength(100); // Novo campo
        builder.Property(c => c.Street).HasMaxLength(200);
        builder.Property(c => c.Number).HasMaxLength(20);
        builder.Property(c => c.Latitude).HasColumnType("float");
        builder.Property(c => c.Longitude).HasColumnType("float");
        builder.Property(c => c.Slogan).HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.PhoneNumber).IsUnique();
        builder.HasIndex(c => c.City);
        builder.HasIndex(c => c.Neighborhood);
    }
}
