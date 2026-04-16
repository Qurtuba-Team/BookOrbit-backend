namespace BookOrbit.Application.Features.Students.Commands.StateMachien.PendStudent;
public record PendStudentCommand(Guid StudentId)
    :IRequest<Result<Updated>>;