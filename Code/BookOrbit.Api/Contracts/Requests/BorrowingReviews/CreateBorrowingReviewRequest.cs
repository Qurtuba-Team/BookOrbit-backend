namespace BookOrbit.Api.Contracts.Requests.BorrowingReviews;

public record CreateBorrowingReviewRequest
{
    public Guid ReviewerStudentId { get; set; } = Guid.Empty;
    public Guid ReviewedStudentId { get; set; } = Guid.Empty;
    public string? Description { get; set; } = null;
    public int Rating { get; set; }
}
