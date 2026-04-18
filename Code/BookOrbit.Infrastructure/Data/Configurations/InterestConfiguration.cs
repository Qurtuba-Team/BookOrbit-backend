namespace BookOrbit.Infrastructure.Data.Configurations;

public class InterestConfiguration : IEntityTypeConfiguration<Interest>
{
    public void Configure(EntityTypeBuilder<Interest> builder)
    {
        builder.ToTable("Interests");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Type)
            .IsRequired();

        builder.HasIndex(i => i.Type)
            .IsUnique();

        builder.Property(i => i.Name)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();
    }
}
