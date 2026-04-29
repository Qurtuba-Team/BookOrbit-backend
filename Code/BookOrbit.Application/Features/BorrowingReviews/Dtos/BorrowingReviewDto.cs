
namespace BookOrbit.Application.Features.BorrowingReviews.Dtos;

public record BorrowingReviewDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid ReviewerStudentId { get; set; } = Guid.Empty;
    public Guid ReviewedStudentId { get; set; } = Guid.Empty;
    public Guid BorrowingTransactionId { get; set; } = Guid.Empty;
    public string? Description { get; set; } = null;
    public int Rating { get; set; }

    [JsonConstructor]
    private BorrowingReviewDto() { }

    private BorrowingReviewDto(
        Guid id,
        Guid reviewerStudentId,
        Guid reviewedStudentId,
        Guid borrowingTransactionId,
        string? description,
        int rating)
    {
        Id = id;
        ReviewerStudentId = reviewerStudentId;
        ReviewedStudentId = reviewedStudentId;
        BorrowingTransactionId = borrowingTransactionId;
        Description = description;
        Rating = rating;
    }

    public static BorrowingReviewDto FromEntity(BorrowingReview review)
        => new(
            review.Id,
            review.ReviewerStudentId,
            review.ReviewedStudentId,
            review.BorrowingTransactionId,
            review.Description,
            review.Rating.Value);
}
