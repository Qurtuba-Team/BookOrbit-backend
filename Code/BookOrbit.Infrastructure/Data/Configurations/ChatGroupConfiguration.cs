using BookOrbit.Domain.ChatGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookOrbit.Infrastructure.Data.Configurations;

public class ChatGroupConfiguration : IEntityTypeConfiguration<ChatGroup>
{
    public void Configure(EntityTypeBuilder<ChatGroup> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("ChatGroups");

        builder.HasKey(cg => cg.Id);

        builder.Property(cg => cg.Id)
            .ValueGeneratedNever();

        builder.Property(cg => cg.Student1Id)
            .IsRequired();

        builder.Property(cg => cg.Student2Id)
            .IsRequired();

        builder.HasOne(cg => cg.Student1)
            .WithMany()
            .HasForeignKey(cg => cg.Student1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cg => cg.Student2)
            .WithMany()
            .HasForeignKey(cg => cg.Student2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cg => new { cg.Student1Id, cg.Student2Id })
            .IsUnique();
    }
}
