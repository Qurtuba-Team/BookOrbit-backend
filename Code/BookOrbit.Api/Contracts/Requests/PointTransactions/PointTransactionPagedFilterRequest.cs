
namespace BookOrbit.Api.Contracts.Requests.PointTransactions;

public record PointTransactionPagedFilterRequest : PagedFilterRequest
{
    public Guid? StudentId { get; set; } = null;
    public Guid? BorrowingReviewId { get; set; } = null;
    public List<PointTransactionReason>? Reasons { get; set; } = null;
}
