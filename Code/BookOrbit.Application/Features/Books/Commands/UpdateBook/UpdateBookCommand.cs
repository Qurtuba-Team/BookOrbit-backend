namespace BookOrbit.Application.Features.Books.Commands.UpdateBook;
public record UpdateBookCommand(
    Guid Id,
    string Title,
    string BookCoverImageFileName,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
