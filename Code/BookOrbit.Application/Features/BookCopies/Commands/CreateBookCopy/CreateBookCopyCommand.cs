namespace BookOrbit.Application.Features.BookCopies.Commands.CreateBookCopy;
public record CreateBookCopyCommand (
    Guid OwnerId,
    Guid BookId,
    BookCopyCondition Condition): IRequest<Result<BookCopyDtoWithBookDetails>>;