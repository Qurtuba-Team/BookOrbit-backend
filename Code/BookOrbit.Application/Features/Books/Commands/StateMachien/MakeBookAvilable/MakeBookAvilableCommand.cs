namespace BookOrbit.Application.Features.Books.Commands.StateMachien.MakeBookAvilable;

public record MakeBookAvilableCommand(Guid BookId) : IRequest<Result<Updated>>;
