namespace BookOrbit.Api.Contracts.Requests.Students;

public sealed record RejectStudentRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
