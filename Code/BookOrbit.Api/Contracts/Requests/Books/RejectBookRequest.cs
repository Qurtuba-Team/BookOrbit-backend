namespace BookOrbit.Api.Contracts.Requests.Books;

public sealed record RejectBookRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
