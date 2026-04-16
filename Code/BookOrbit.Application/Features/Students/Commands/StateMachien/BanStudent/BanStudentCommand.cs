namespace BookOrbit.Application.Features.Students.Commands.StateMachien.BanStudent;
public record BanStudentCommand(Guid StudentId):IRequest<Result<Updated>>;