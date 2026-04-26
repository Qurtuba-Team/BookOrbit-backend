namespace BookOrbit.Application.Features.Students.Dtos;

public record StudentDto
{

    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public int Points { get; set; }
    public StudentState State { get; set; }
    public DateTimeOffset? JoinDate { get; set; }

    [JsonConstructor]
    private StudentDto() { }


    private StudentDto(
    Guid id,
    string name,
    int points,
    StudentState state,
    DateTimeOffset? joinDate)
    {
        Id = id;
        Name = name;
        Points = points;
        State = state;
        JoinDate = joinDate;
    }

    static public StudentDto FromEntity(Student entity)
    =>
        new(
            entity.Id,
            entity.Name.Value,
            entity.Points.Value,
            entity.State,
            entity.JoinDateUtc);

}