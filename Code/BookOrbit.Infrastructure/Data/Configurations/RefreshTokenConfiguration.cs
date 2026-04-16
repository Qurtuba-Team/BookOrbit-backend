namespace BookOrbit.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration:IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(rt => rt.ExpiresOnUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.HasOne<AppUser>()
           .WithMany()
           .HasForeignKey(x => x.UserId)
           .HasPrincipalKey(x => x.Id)
           .OnDelete(DeleteBehavior.Cascade);
    }
}
