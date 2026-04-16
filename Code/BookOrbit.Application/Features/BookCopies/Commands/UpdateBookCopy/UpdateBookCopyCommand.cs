namespace BookOrbit.Application.Features.BookCopies.Commands.UpdateBookCopy;
public record UpdateBookCopyCommand(
    Guid Id,
    BookCopyCondition Condition):IRequest<Result<Updated>>;