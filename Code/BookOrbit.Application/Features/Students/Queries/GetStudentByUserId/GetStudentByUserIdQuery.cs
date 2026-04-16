namespace BookOrbit.Application.Features.Students.Queries.GetStudentByUserId;
public record GetStudentByUserIdQuery(
    string? UserId) : IRequest<Result<StudentDto>>;

