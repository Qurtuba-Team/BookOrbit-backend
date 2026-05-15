namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeUnAvilableBookCopy;
public record MakeUnAvilableBookCopyCommand(Guid BookCopyId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
