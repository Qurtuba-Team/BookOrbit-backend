namespace BookOrbit.Api.Contracts.Requests.BookCopies;

public sealed record MakeBookCopyAvailableRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
