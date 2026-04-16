namespace BookOrbit.Application.Features.Books.Commands.CreateBook;
public record CreateBookCommand(
    string Title,
    string ISBN,
    string Publisher,
    BookCategory Category,
    string Author,
    string CoverImageFileName):IRequest<Result<BookDto>>;