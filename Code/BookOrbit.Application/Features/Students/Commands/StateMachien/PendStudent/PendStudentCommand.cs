namespace BookOrbit.Application.Features.Students.Commands.StateMachien.PendStudent;

public record PendStudentCommand(
    Guid StudentId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
