namespace BookOrbit.Api.Contracts.Requests.Students;

public record CreateStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = null;
    public string? TelegramUserId { get; set; } = null;
    public IFormFile PersonalPhoto { get; set; } = default!;
    public string UniversityMailAddress { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}