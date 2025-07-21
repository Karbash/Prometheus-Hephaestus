using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.ToTable("conversation_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.LastIntent)
            .HasColumnName("last_intent")
            .HasMaxLength(100);

        builder.Property(s => s.ConversationStep)
            .HasColumnName("conversation_step")
            .HasMaxLength(100);

        builder.Property(s => s.SessionDataJson)
            .HasColumnName("session_data_json")
            .HasColumnType("text");

        builder.Property(s => s.LastActivity)
            .HasColumnName("last_activity")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Índices para performance
        builder.HasIndex(s => s.SessionId).IsUnique();
        builder.HasIndex(s => s.PhoneNumber);
        builder.HasIndex(s => s.LastActivity);
        builder.HasIndex(s => s.IsActive);

        // Relacionamento com mensagens
        builder.HasMany(s => s.Messages)
            .WithOne()
            .HasForeignKey(m => m.SessionId)
            .HasPrincipalKey(s => s.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 