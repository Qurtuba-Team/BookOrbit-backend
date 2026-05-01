namespace BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.DomainEvents;

public class BorrowingReviewCreatedEvent(Guid borrowingReviewId, Guid reviewedStudentId, int ratingValue) : DomainEvent
{
    public Guid BorrowingReviewId { get; } = borrowingReviewId;
    public Guid ReviewedStudentId { get; } = reviewedStudentId;
    public int RatingValue { get; } = ratingValue;
}
