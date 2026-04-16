namespace BookOrbit.Application.Features.Students.Commands.StateMachien.RejectStudent;
public record RejectStudentCommand(Guid StudentId):IRequest<Result<Updated>>;