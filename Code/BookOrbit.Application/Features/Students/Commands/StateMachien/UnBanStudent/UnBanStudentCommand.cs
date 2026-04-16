namespace BookOrbit.Application.Features.Students.Commands.StateMachien.UnBanStudent;
public record UnBanStudentCommand(Guid StudentId):IRequest<Result<Updated>>;