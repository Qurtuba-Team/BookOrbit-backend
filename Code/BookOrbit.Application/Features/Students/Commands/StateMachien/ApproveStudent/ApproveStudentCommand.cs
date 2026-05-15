namespace BookOrbit.Application.Features.Students.Commands.StateMachien.ApproveStudent;

public record ApproveStudentCommand(
    Guid StudentId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
