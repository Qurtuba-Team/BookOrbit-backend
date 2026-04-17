namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeUnAvilableBookCopy;
public record MakeUnAvilableBookCopyCommand(Guid BookCopyId) : IRequest<Result<Updated>>;
