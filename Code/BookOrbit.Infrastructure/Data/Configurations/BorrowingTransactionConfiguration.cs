using BookOrbit.Domain.BorrowingTransactions;

namespace BookOrbit.Infrastructure.Data.Configurations;
public class BorrowingTransactionConfiguration : IEntityTypeConfiguration<BorrowingTransaction>
{
    public void Configure(EntityTypeBuilder<BorrowingTransaction> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("BorrowingTransactions");

        builder.HasKey(bt => bt.Id);

        builder.Property(bt => bt.Id)
            .ValueGeneratedNever();

        builder.Property(bt => bt.BorrowingRequestId)
            .IsRequired();

        builder.Property(bt => bt.LenderStudentId)
            .IsRequired();

        builder.Property(bt => bt.BorrowerStudentId)
            .IsRequired();

        builder.Property(bt => bt.BookCopyId)
            .IsRequired();

        builder.Property(bt => bt.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(bt => bt.ExpectedReturnDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(bt => bt.ActualReturnDate)
            .HasColumnType("datetimeoffset")
            .IsRequired(false);

        builder.HasOne(bt => bt.BorrowingRequest)
            .WithMany()
            .HasForeignKey(bt => bt.BorrowingRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bt => bt.LenderStudent)
            .WithMany()
            .HasForeignKey(bt => bt.LenderStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bt => bt.BorrowerStudent)
            .WithMany()
            .HasForeignKey(bt => bt.BorrowerStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bt => bt.BookCopy)
            .WithMany()
            .HasForeignKey(bt => bt.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bt => bt.BorrowingRequestId);
        builder.HasIndex(bt => bt.LenderStudentId);
        builder.HasIndex(bt => bt.BorrowerStudentId);
        builder.HasIndex(bt => bt.BookCopyId);
        builder.HasIndex(bt => bt.State);
    }
}
