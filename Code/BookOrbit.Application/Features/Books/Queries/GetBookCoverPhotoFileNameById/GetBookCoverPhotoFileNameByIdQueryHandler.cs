namespace BookOrbit.Application.Features.Books.Queries.GetBookCoverPhotoFileNameById;
public class GetBookCoverPhotoFileNameByIdQueryHandler(
    IAppDbContext context) : IRequestHandler<GetBookCoverPhotoFileNameByIdQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetBookCoverPhotoFileNameByIdQuery query, CancellationToken ct)
    {
        var book = await context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.BookId, ct);

        if (book is null)
        {
            return BookApplicationErrors.NotFoundById;
        }

        return book.CoverImageFileName;
    }
}
