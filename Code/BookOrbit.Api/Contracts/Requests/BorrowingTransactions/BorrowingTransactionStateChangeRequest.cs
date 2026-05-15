namespace BookOrbit.Api.Contracts.Requests.BorrowingTransactions;
public record BorrowingTransactionStateChangeRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
