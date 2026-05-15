namespace BookOrbit.Application.Features.Books.Commands.StateMachien.RejectBook;

public record RejectBookCommand(Guid BookId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
