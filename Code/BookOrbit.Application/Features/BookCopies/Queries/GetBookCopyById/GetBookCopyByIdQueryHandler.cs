
namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopyById;
public class GetBookCopyByIdQueryHandler(
    ILogger<GetBookCopyByIdQueryHandler> logger,
    IAppDbContext context,
    IRouteService routeService) : IRequestHandler<GetBookCopyByIdQuery, Result<BookCopyDtoWithBookDetails>>
{
    public async Task<Result<BookCopyDtoWithBookDetails>> Handle(GetBookCopyByIdQuery query, CancellationToken ct)
    {
        var bookCopy = await context.BookCopies
            .AsNoTracking()
            .Include(b=>b.Owner)
            .Include(b=>b.Book)
            .FirstOrDefaultAsync(
            b => b.Id == query.BookCopyId,ct);

        if (bookCopy is null)
        {
            logger.LogWarning("Book Copy {BookCopyId} not found.", query.BookCopyId);

            return BookCopyApplicationErrors.NotFoundById;
        }

        bool isListed = await context.LendingListRecords.AnyAsync(l =>
l.BookCopyId == query.BookCopyId&&
        (l.State == LendingListRecordState.Available
        ||
        l.State == LendingListRecordState.Reserved
        ||
        l.State == LendingListRecordState.Borrowed), ct);

        string bookCoverImageUrl = routeService.GetBookCoverImageRoute(bookCopy.Book!.CoverImageFileName);

        return BookCopyDtoWithBookDetails.FromEntity(
            bookCopy,
            bookCopy.Owner!.Name.Value,
            BookDto.FromEntity(bookCopy.Book!, bookCoverImageUrl),
            isListed);
    }
}
