namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeAvilableBookCopy;
public record MakeAvilableBookCopyCommand(Guid BookCopyId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
