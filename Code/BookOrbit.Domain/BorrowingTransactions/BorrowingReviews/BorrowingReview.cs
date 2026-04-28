using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;

namespace BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;

public class BorrowingReview : AuditableEntity
{
    public Guid ReviewerStudentId { get; }
    public Guid ReviewedStudentId { get; }
    public Guid BorrowingTransactionId { get; }

    public string? Description { get; }
    public StartsRating Rating { get; }

#pragma warning disable CS8618
    private BorrowingReview() { }

    private BorrowingReview(
        Guid id,
        Guid reviewerStudentId,
        Guid reviewedStudentId,
        Guid borrowingTransactionId,
        string? description,
        StartsRating rating) : base(id)
    {
        ReviewerStudentId = reviewerStudentId;
        ReviewedStudentId = reviewedStudentId;
        BorrowingTransactionId = borrowingTransactionId;
        Description = string.IsNullOrWhiteSpace(description)?
            null:
            description;
        Rating = rating;
    }

    static public Result<BorrowingReview> Create(
        Guid id,
        Guid reviewerStudentId,
        Guid reviewedStudentId,
        Guid borrowingTransactionId,
        string? description,
        StartsRating rating)
    {
        if (id == Guid.Empty)
            return BorrowingReviewErrors.IdRequired;

        if (reviewerStudentId == Guid.Empty)
            return BorrowingReviewErrors.ReviewerStudentIdRequired;

        if (reviewedStudentId == Guid.Empty)
            return BorrowingReviewErrors.ReviewedStudentIdRequired;

        if (borrowingTransactionId == Guid.Empty)
            return BorrowingReviewErrors.BorrowingTransactionIdRequired;

        if (rating is null)
            return BorrowingReviewErrors.RatingRequired;


        return new BorrowingReview(
            id,
            reviewerStudentId,
            reviewedStudentId,
            borrowingTransactionId,
            description,
            rating);
    }
}

