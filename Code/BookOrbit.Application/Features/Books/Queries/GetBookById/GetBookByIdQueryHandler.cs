namespace BookOrbit.Application.Features.Books.Queries.GetBookById;
public class GetBookByIdQueryHandler(
    ILogger<GetBookByIdQueryHandler> logger,
    IAppDbContext context) : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken ct)
    {
        var book = await context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == query.BookId,ct);

        if (book is null)
        {
            logger.LogWarning("Book {BookId} not found .", query.BookId);

            return BookApplicationErrors.NotFoundById;
        }

        return BookDto.FromEntity(book);
    }
}