

namespace BookOrbit.Application.Features.Students.Dtos;

public record StudentListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = null;
    public string? TelegramUserId { get; set; } = null;
    public string UniversityMailAddress { get; set; } = string.Empty;
    public int Points { get; set; }
    public StudentState State { get; set; }
    public DateTimeOffset? JoinDate { get; set; }

    [JsonConstructor]
    private StudentListItemDto() { }

    private StudentListItemDto(
  Guid id,
  string name,
  string? phoneNumber,
  string? telegramUserId,
  string universityMailAddress,
  int points,
  StudentState state,
  DateTimeOffset? joinDate)
    {
        Id = id;
        Name = name;
        PhoneNumber = phoneNumber;
        TelegramUserId = telegramUserId;
        UniversityMailAddress = universityMailAddress;
        Points = points;
        State = state;
        JoinDate = joinDate;
    }

    public static Expression<Func<Student, StudentListItemDto>> Projection =>
    s => new StudentListItemDto(
        s.Id,
        s.Name.Value,
        s.PhoneNumber != null ? s.PhoneNumber.Value : null,
        s.TelegramUserId != null ? s.TelegramUserId.Value : null,
        s.UniversityMail.Value,
        s.Points,
        s.State,
        s.JoinDateUtc
    );

}
