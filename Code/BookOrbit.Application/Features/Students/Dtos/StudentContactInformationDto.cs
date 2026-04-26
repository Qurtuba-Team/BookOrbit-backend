namespace BookOrbit.Application.Features.Students.Dtos;
public record StudentContactInformationDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string? PhoneNumber { get; set; } = null;
    public string? TelegramUserId { get; set; } = null;


    public StudentContactInformationDto(Guid id, string? phoneNumber, string? telegramUserId)
    {
        Id = id;
        PhoneNumber = phoneNumber;
        TelegramUserId = telegramUserId;
    }
}