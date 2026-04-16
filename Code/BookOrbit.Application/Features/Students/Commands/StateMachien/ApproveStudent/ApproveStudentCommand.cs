namespace BookOrbit.Application.Features.Students.Commands.StateMachien.ApproveStudent;
public record ApproveStudentCommand(Guid StudentId) : IRequest<Result<Updated>>;