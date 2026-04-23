namespace BookOrbit.Application.Features.Books.Commands.StateMachien.MakeBookAvilable;
public class MakeBookAvilableCommandHandler(
    IAppDbContext context,
    ILogger<MakeBookAvilableCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<MakeBookAvilableCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MakeBookAvilableCommand command, CancellationToken ct)
    {
        var book = await context.Books
            .FirstOrDefaultAsync(b => b.Id == command.BookId, ct);

        if (book is null)
        {
            logger.LogWarning("Book {BookId} not found for making available.", command.BookId);
            return BookApplicationErrors.NotFoundById;
        }

        var availableResult = book.MarkAsAvailable();

        if (availableResult.IsFailure)
            return availableResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCachingConstants.BookTag, ct);

        logger.LogInformation("Book {BookId} has been marked as available.", book.Id);

        return Result.Updated;
    }
}
