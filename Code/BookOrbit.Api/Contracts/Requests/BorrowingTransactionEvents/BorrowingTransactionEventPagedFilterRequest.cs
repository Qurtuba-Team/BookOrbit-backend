namespace BookOrbit.Api.Contracts.Requests.BorrowingTransactionEvents;

public record BorrowingTransactionEventPagedFilterRequest : PagedFilterRequest
{
    public Guid? BorrowingTransactionId { get; set; } = null;
    public List<BorrowingTransactionState>? States { get; set; } = null;
}
