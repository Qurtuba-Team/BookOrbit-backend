namespace BookOrbit.Api.Contracts.Requests.Students;

public record UpdateStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public IFormFile? PersonalPhoto { get; set; } = null;
}