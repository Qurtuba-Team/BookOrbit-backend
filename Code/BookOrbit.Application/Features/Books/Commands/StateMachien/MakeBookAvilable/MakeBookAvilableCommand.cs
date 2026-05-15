namespace BookOrbit.Application.Features.Books.Commands.StateMachien.MakeBookAvilable;

public record MakeBookAvilableCommand
(Guid BookId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
