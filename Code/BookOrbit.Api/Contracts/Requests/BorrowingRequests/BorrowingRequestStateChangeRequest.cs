namespace BookOrbit.Api.Contracts.Requests.BorrowingRequests;

public sealed record BorrowingRequestStateChangeRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
