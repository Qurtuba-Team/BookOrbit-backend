namespace BookOrbit.Application.Features.Books.Commands.DeleteBook;
public class DeleteBookCommandHandler(
    ILogger<DeleteBookCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser user) : IRequestHandler<DeleteBookCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(
        DeleteBookCommand command,
        CancellationToken ct)
    {
        var bookTitle = await context.Books.AsNoTracking().
            Where(b => b.Id == command.Id)
            .Select(b => b.Title)
            .FirstOrDefaultAsync(ct);

        if (bookTitle is null)
        {
            logger.LogWarning("Book {BookId} not found for delete.", command.Id);

            return BookApplicationErrors.NotFoundById;
        }

        var isUsed = await context.BookCopies.AnyAsync(
            bc => bc.BookId == command.Id, ct);

        if (isUsed)
        {
            logger.LogWarning("Book {BookId} is used by book copies and cannot be deleted.", command.Id);
            return BookApplicationErrors.IsUsedByBookCopies;
        }
        await context.Books
            .Where(b => b.Id == command.Id)
            .ExecuteDeleteAsync(ct); // Perform the delete operation directly in the database without loading the entity into memory

        await cache.RemoveByTagAsync(BookCachingConstants.BookTag, ct);

        logger.LogInformation(
            "Book deleted successfully. Id: {BookId}, Title: {Title} by [{UserId}]",
            command.Id,
            bookTitle,
            user.Id);

        return Result.Deleted;
    }
}