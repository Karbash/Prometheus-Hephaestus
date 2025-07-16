using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(c => c.ApiKey)
            .HasColumnName("api_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(c => c.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();

        builder.Property(c => c.FeeType)
            .HasColumnName("fee_type")
            .IsRequired();

        builder.Property(c => c.FeeValue)
            .HasColumnName("fee_value")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(c => c.MfaSecret)
            .HasColumnName("mfa_secret")
            .HasMaxLength(32);

        builder.Property(c => c.Slogan)
            .HasColumnName("slogan")
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.AddressId)
            .HasColumnName("address_id")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.ApiKey).IsUnique();
        builder.HasIndex(c => c.IsEnabled);
        builder.HasIndex(c => c.FeeType);
        builder.HasIndex(c => c.CreatedAt);
    }
}