namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeAvilableBookCopy;
public record MakeAvilableBookCopyCommand(Guid BookCopyId) : IRequest<Result<Updated>>;
