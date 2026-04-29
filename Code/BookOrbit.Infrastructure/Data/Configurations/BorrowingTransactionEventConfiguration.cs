
namespace BookOrbit.Infrastructure.Data.Configurations;

public class BorrowingTransactionEventConfiguration : IEntityTypeConfiguration<BorrowingTransactionEvent>
{
    public void Configure(EntityTypeBuilder<BorrowingTransactionEvent> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("BorrowingTransactionEvents");

        builder.HasKey(bte => bte.Id);

        builder.Property(bte => bte.Id)
            .ValueGeneratedNever();

        builder.Property(bte => bte.BorrowingTransactionId)
            .IsRequired();

        builder.Property(bte => bte.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<BorrowingTransaction>()
            .WithMany()
            .HasForeignKey(bte => bte.BorrowingTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bte => bte.BorrowingTransactionId);
        builder.HasIndex(bte => bte.State);
    }
}
