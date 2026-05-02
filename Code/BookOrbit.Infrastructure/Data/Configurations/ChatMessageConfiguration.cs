using BookOrbit.Domain.ChatMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookOrbit.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("ChatMessages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Id)
            .ValueGeneratedNever();

        builder.Property(cm => cm.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cm => cm.SenderId)
            .IsRequired();

        builder.Property(cm => cm.ChatGroupId)
            .IsRequired();

        builder.Property(cm => cm.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.ChatGroup)
            .WithMany()
            .HasForeignKey(cm => cm.ChatGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cm => cm.ChatGroupId);
        builder.HasIndex(cm => cm.CreatedAtUtc);
    }
}
