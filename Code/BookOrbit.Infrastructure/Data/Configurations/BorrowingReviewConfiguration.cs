
namespace BookOrbit.Infrastructure.Data.Configurations;

public class BorrowingReviewConfiguration : IEntityTypeConfiguration<BorrowingReview>
{
    public void Configure(EntityTypeBuilder<BorrowingReview> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("BorrowingReviews");

        builder.HasKey(br => br.Id);

        builder.Property(br => br.Id)
            .ValueGeneratedNever();

        builder.Property(br => br.ReviewerStudentId)
            .IsRequired();

        builder.Property(br => br.ReviewedStudentId)
            .IsRequired();

        builder.Property(br => br.BorrowingTransactionId)
            .IsRequired();

        builder.Property(br => br.Description)
            .IsRequired(false);

        builder.OwnsOne(br => br.Rating, r =>
        {
            r.Property(x => x.Value)
                .HasColumnName("Rating")
                .IsRequired();
        });

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(br => br.ReviewerStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(br => br.ReviewedStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BorrowingTransaction>()
            .WithMany()
            .HasForeignKey(br => br.BorrowingTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(br => br.ReviewerStudentId);
        builder.HasIndex(br => br.ReviewedStudentId);
        builder.HasIndex(br => br.BorrowingTransactionId);
    }
}
