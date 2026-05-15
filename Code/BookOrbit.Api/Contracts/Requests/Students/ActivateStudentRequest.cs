namespace BookOrbit.Api.Contracts.Requests.Students;

public sealed record ActivateStudentRequest : ConcurrencyRequest
{
    public override string RowVersion { get; set; } = string.Empty;
}
