namespace BookOrbit.Application.Features.Books.Commands.StateMachien.RejectBook;

public record RejectBookCommand(Guid BookId) : IRequest<Result<Updated>>;
