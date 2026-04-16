namespace BookOrbit.Application.Features.Students.Queries.GetStudentPersonalPhotoFileNameById;
public record GetStudentPersonalPhotoFileNameByIdQuery(
    Guid StudentId) : IRequest<Result<string>>;
