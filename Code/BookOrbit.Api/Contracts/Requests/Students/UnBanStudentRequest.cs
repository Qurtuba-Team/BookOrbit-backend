namespace BookOrbit.Api.Contracts.Requests.Students;

public sealed record UnBanStudentRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
