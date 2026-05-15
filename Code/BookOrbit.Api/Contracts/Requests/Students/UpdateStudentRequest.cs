namespace BookOrbit.Api.Contracts.Requests.Students;

public record UpdateStudentRequest : ConcurrencyRequest
{
    public string Name { get; set; } = string.Empty;
    public IFormFile? PersonalPhoto { get; set; } = null;
    public override string RowVersion { get; set; } = string.Empty;
}