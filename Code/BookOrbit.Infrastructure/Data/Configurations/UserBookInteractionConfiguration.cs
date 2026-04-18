namespace BookOrbit.Infrastructure.Data.Configurations;

public class UserBookInteractionConfiguration : IEntityTypeConfiguration<UserBookInteraction>
{
    public void Configure(EntityTypeBuilder<UserBookInteraction> builder)
    {
        builder.ToTable("UserBookInteractions");

        builder.HasKey(ubi => ubi.Id);

        builder.Property(ubi => ubi.Id)
            .ValueGeneratedNever();

        builder.Property(ubi => ubi.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(ubi => ubi.Rating)
            .IsRequired(false);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_UserBookInteractions_Rating",
            "[Rating] IS NULL OR ([Rating] >= 1 AND [Rating] <= 5)"));

        builder.Property(ubi => ubi.IsRead)
            .IsRequired();

        builder.Property(ubi => ubi.IsWishlisted)
            .IsRequired();

        builder.Property(ubi => ubi.InteractionDate)
            .IsRequired();

        builder.HasIndex(ubi => new { ubi.UserId, ubi.BookId });

        builder.HasOne(ubi => ubi.Book)
            .WithMany()
            .HasForeignKey(ubi => ubi.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany(u => u.Interactions)
            .HasForeignKey(ubi => ubi.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
