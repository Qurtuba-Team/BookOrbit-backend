namespace BookOrbit.Application.Features.Students.Commands.StateMachien.ActivateStudent;
public record ActivateStudentCommand(Guid StudentId):IRequest<Result<Updated>>;