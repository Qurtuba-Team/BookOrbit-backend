namespace BookOrbit.Application.Features.Books.Commands.StateMachien.RejectBook;
public class RejectBookCommandHandler(
    IAppDbContext context,
    ILogger<RejectBookCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RejectBookCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RejectBookCommand command, CancellationToken ct)
    {
        var book = await context.Books
            .FirstOrDefaultAsync(b => b.Id == command.BookId, ct);

        if (book is null)
        {
            logger.LogWarning("Book {BookId} not found for rejecting.", command.BookId);
            return BookApplicationErrors.NotFoundById;
        }

        var rejectResult = book.MarkAsRejected();

        if (rejectResult.IsFailure)
            return rejectResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCachingConstants.BookTag, ct);

        logger.LogInformation("Book {BookId} has been rejected.", book.Id);

        return Result.Updated;
    }
}
