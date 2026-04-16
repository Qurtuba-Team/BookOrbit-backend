namespace BookOrbit.Application.Features.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(
    Guid Id,
    string Name,
    string personalPhotoFileName) : IRequest<Result<Updated>>;