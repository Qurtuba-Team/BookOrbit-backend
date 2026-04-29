
namespace BookOrbit.Api.Contracts.Requests.BorrowingTransactions;
public record BorrowingTransactionPagedFilterRequest : PagedFilterRequest
{
    public Guid? BorrowerStudentId { get; set; } = null;
    public Guid? LenderStudentId { get; set; } = null;
    public Guid? BookCopyId { get; set; } = null;
    public Guid? BorrowingRequestId { get; set; } = null;
    public List<BorrowingTransactionState>? States { get; set; } = null;
}
