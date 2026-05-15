namespace BookOrbit.Api.Contracts.Requests.LendingListRecords;

public sealed record CloseLendingListRecordRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
