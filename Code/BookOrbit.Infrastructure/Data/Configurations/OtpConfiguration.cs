using BookOrbit.Domain.Otps;

namespace BookOrbit.Infrastructure.Data.Configurations;
public class OtpConfiguration : IEntityTypeConfiguration<Otp>
{
    public void Configure(EntityTypeBuilder<Otp> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("Otps");

        builder.HasKey(otp => otp.Id);

        builder.Property(otp => otp.Id)
            .ValueGeneratedNever();

        builder.Property(otp => otp.ExpirationDateUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(otp => otp.Code)
            .HasMaxLength(Otp.Length)
            .IsRequired();

        builder.Property(otp => otp.TargetId)
            .IsRequired();

        builder.Property(otp=>otp.IsUsed)
            .IsRequired();

        builder.Property(otp=>otp.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(otp => otp.Code);
    }
}