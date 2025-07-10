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

        builder.Property(c => c.Id).IsRequired();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(255);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(c => c.ApiKey).IsRequired().HasMaxLength(100);
        builder.Property(c => c.PasswordHash).IsRequired();
        builder.Property(c => c.Role).IsRequired().HasConversion<string>();
        builder.Property(c => c.IsEnabled).IsRequired();
        builder.Property(c => c.FeeType).IsRequired().HasConversion<string>();
        builder.Property(c => c.FeeValue).IsRequired().HasPrecision(18, 2);
        builder.Property(c => c.MfaSecret).HasMaxLength(100);

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.PhoneNumber).IsUnique();
    }
}
