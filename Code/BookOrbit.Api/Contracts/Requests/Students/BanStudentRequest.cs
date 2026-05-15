namespace BookOrbit.Api.Contracts.Requests.Students;

public sealed record BanStudentRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
