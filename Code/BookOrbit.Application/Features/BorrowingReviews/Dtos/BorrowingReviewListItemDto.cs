using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;

namespace BookOrbit.Application.Features.BorrowingReviews.Dtos;

public record BorrowingReviewListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid ReviewerStudentId { get; set; } = Guid.Empty;
    public Guid ReviewedStudentId { get; set; } = Guid.Empty;
    public Guid BorrowingTransactionId { get; set; } = Guid.Empty;
    public string? Description { get; set; } = null;
    public int Rating { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private BorrowingReviewListItemDto() { }

    private BorrowingReviewListItemDto(
        Guid id,
        Guid reviewerStudentId,
        Guid reviewedStudentId,
        Guid borrowingTransactionId,
        string? description,
        int rating,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        ReviewerStudentId = reviewerStudentId;
        ReviewedStudentId = reviewedStudentId;
        BorrowingTransactionId = borrowingTransactionId;
        Description = description;
        Rating = rating;
        CreatedAtUtc = createdAtUtc;
    }

    public static BorrowingReviewListItemDto FromEntity(BorrowingReview review)
        => new(
            review.Id,
            review.ReviewerStudentId,
            review.ReviewedStudentId,
            review.BorrowingTransactionId,
            review.Description,
            review.Rating.Value,
            review.CreatedAtUtc);
}
