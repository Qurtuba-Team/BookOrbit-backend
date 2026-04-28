namespace BookOrbit.Api.Contracts.Requests.BorrowingReviews;

public record BorrowingReviewPagedFilterRequest : PagedFilterRequest
{
    public Guid? ReviewerStudentId { get; set; } = null;
    public Guid? ReviewedStudentId { get; set; } = null;
    public Guid? BorrowingTransactionId { get; set; } = null;
}
