
namespace BookOrbit.Infrastructure.Data.Configurations;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("PointTransactions");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Id)
            .ValueGeneratedNever();

        builder.Property(pt => pt.StudentId)
            .IsRequired();

        builder.Property(pt => pt.BorrowingReviewId)
            .IsRequired(false);

        builder.Property(pt => pt.Points)
            .IsRequired();

        builder.Property(pt => pt.Reason)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(pt => pt.StudentId);
        builder.HasIndex(pt => pt.BorrowingReviewId);
        builder.HasIndex(pt => pt.Reason);
    }
}
