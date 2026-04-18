namespace BookOrbit.Infrastructure.Data.Configurations;

public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("UserInterests");

        builder.HasKey(ui => new { ui.UserId, ui.InterestId });

        builder.Property(ui => ui.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(ui => ui.Interest)
            .WithMany(i => i.UserInterests)
            .HasForeignKey(ui => ui.InterestId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to AspNetUsers managed by shadow property — no CLR nav back to AppUser in domain entity
        builder.HasOne<AppUser>()
            .WithMany(u => u.UserInterests)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
