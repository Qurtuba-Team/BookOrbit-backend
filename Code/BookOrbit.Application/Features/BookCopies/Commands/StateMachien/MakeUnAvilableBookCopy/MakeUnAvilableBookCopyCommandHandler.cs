namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeUnAvilableBookCopy;
public class MakeUnAvilableBookCopyCommandHandler(
    IAppDbContext context,
    ILogger<MakeUnAvilableBookCopyCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<MakeUnAvilableBookCopyCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MakeUnAvilableBookCopyCommand command, CancellationToken ct)
    {
        var bookCopy = await context.BookCopies
            .FirstOrDefaultAsync(bc => bc.Id == command.BookCopyId, ct);

        if (bookCopy is null)
        {
            logger.LogWarning(
                "Book copy {BookCopyId} not found for making unavailable.",
                command.BookCopyId);

            return BookCopyApplicationErrors.NotFoundById;
        }

        var unavailableResult = bookCopy.MarkAsUnAvilable();

        if (unavailableResult.IsFailure)
            return unavailableResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCopyCachingConstants.BookCopyTag, ct);

        logger.LogInformation(
            "Book copy {BookCopyId} has been marked as unavailable",
            bookCopy.Id);

        return Result.Updated;
    }
}
