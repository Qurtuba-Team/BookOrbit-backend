namespace BookOrbit.Application.Features.Books.Commands.DeleteBook;
public record DeleteBookCommand
    (Guid Id):IRequest<Result<Deleted>>;
