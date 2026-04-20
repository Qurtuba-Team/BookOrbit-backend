namespace BookOrbit.Application.Features.Students.Dtos;

public record StudentDto
{

    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string UniversityMailAddress { get; set; } = string.Empty;
    public int Points { get; set; }
    public StudentState State { get; set; }
    public DateTimeOffset? JoinDate { get; set; }

    [JsonConstructor]
    private StudentDto() { }


    private StudentDto(
    Guid id,
    string name,
    string universityMailAddress,
    int points,
    StudentState state,
    DateTimeOffset? joinDate)
    {
        Id = id;
        Name = name;
        UniversityMailAddress = universityMailAddress;
        Points = points;
        State = state;
        JoinDate = joinDate;
    }

    static public StudentDto FromEntity(Student entity)
    =>
        new(
            entity.Id,
            entity.Name.Value,
            entity.UniversityMail.Value,
            entity.Points,
            entity.State,
            entity.JoinDateUtc);

}