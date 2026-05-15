namespace BookOrbit.Application.Features.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(
    Guid Id,
    string Name,
    string PersonalPhotoFileName,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);