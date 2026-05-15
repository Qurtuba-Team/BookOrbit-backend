namespace BookOrbit.Application.Features.Students.Commands.StateMachien.RejectStudent;

public record RejectStudentCommand(
    Guid StudentId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
