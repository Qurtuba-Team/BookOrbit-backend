namespace BookOrbit.Infrastructure.Data.Configurations;
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasMaxLength(15000)
            .IsRequired();

        builder.Property(m => m.RetryCount)
            .IsRequired();

        builder.HasIndex(m => m.State);
        builder.HasIndex(m => m.RetryCount);

    }
}