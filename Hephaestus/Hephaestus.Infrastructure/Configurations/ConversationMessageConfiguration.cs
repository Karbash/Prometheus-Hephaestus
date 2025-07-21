using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hephaestus.Infrastructure.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Message)
            .HasColumnName("message")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(m => m.Intent)
            .HasColumnName("intent")
            .HasMaxLength(100);

        builder.Property(m => m.Response)
            .HasColumnName("response")
            .HasMaxLength(2000);

        builder.Property(m => m.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Property(m => m.UsedOpenAI)
            .HasColumnName("used_openai")
            .IsRequired();

        // Índices para performance
        builder.HasIndex(m => m.SessionId);
        builder.HasIndex(m => m.Timestamp);
        builder.HasIndex(m => m.UsedOpenAI);

        // Chave estrangeira para ConversationSession usando SessionId
        builder.HasOne<ConversationSession>()
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .HasPrincipalKey(s => s.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 