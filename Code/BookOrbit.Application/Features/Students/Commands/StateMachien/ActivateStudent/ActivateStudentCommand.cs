namespace BookOrbit.Application.Features.Students.Commands.StateMachien.ActivateStudent;

public record ActivateStudentCommand(
    Guid StudentId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
