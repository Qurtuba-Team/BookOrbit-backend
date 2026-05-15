namespace BookOrbit.Application.Features.Students.Commands.StateMachien.BanStudent;

public record BanStudentCommand(
    Guid StudentId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
