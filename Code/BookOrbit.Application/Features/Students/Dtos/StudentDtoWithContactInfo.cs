namespace BookOrbit.Application.Features.Students.Dtos;

public record StudentDtoWithContactInfo
{

    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string UniversityMailAddress { get; set; } = string.Empty;
    public int Points { get; set; }
    public StudentState State { get; set; }
    public DateTimeOffset? JoinDate { get; set; }
    public string? PhoneNumber { get; set; } = null;
    public string? TelegramUserId { get; set; } = null;

    [JsonConstructor]
    private StudentDtoWithContactInfo() { }


    private StudentDtoWithContactInfo(
    Guid id,
    string name,
    string universityMailAddress,
    int points,
    StudentState state,
    DateTimeOffset? joinDate,
    string? phoneNumber,
    string? telegramUserId
    )
    {
        Id = id;
        Name = name;
        UniversityMailAddress = universityMailAddress;
        Points = points;
        State = state;
        JoinDate = joinDate;
        PhoneNumber = phoneNumber;
        TelegramUserId = telegramUserId;
    }

    static public StudentDtoWithContactInfo FromEntity(Student entity)
    =>
        new(
            entity.Id,
            entity.Name.Value,
            entity.UniversityMail.Value,
            entity.Points.Value,
            entity.State,
            entity.JoinDateUtc,
            entity.PhoneNumber?.Value,
            entity.TelegramUserId?.Value);

}