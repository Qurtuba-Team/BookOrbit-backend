namespace BookOrbit.Application.Features.Students.Commands.StateMachien.UnBanStudent;

public record UnBanStudentCommand(
    Guid StudentId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
