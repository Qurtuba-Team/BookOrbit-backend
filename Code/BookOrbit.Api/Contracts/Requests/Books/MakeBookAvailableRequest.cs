namespace BookOrbit.Api.Contracts.Requests.Books;

public sealed record MakeBookAvailableRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
