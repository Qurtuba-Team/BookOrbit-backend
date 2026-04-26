namespace BookOrbit.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.PersonalPhotoFileName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.JoinDateUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired(false);

        builder.Property(s => s.State)
           .HasConversion<string>()
           .HasMaxLength(20)
           .IsRequired();

        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.HasOne<AppUser>()
            .WithOne()
            .HasForeignKey<Student>(x => x.UserId)
            .HasPrincipalKey<AppUser>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(s => s.Name, n =>
        {
            n.Property(p => p.Value)
             .HasColumnName("Name")
             .HasMaxLength(StudentName.MaxLength)
             .IsRequired();
        });

        builder.OwnsOne(s => s.PhoneNumber, p =>
        {
            p.Property(x => x.Value)
             .HasColumnName("PhoneNumber")
             .HasMaxLength(PhoneNumber.MaxLength)
             .IsUnicode(false)
             .IsRequired(false);

            p.HasIndex(x => x.Value);
        });

        builder.OwnsOne(s => s.TelegramUserId, t =>
        {
            t.Property(x => x.Value)
             .HasColumnName("TelegramUserId")
             .HasMaxLength(TelegramUserId.MaxLength)
             .IsUnicode(false)
             .IsRequired(false);
        });

        builder.OwnsOne(s => s.UniversityMail, m =>
        {
            m.Property(x => x.Value)
             .HasColumnName("UniversityMail")
             .HasMaxLength(UniversityMail.MaxLength)
             .IsUnicode(false)
             .IsRequired();

            m.HasIndex(x => x.Value).IsUnique();
        });

        builder.OwnsOne(s => s.Points, n =>
        {
            n.Property(p => p.Value)
             .HasColumnName("Points")
             .IsRequired()
             .HasDefaultValue(1);
        });


        builder.Navigation(s => s.PhoneNumber).IsRequired(false);
        builder.Navigation(s => s.TelegramUserId).IsRequired(false);
    }
}
