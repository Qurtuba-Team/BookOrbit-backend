namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeAvilableBookCopy;
public class MakeAvilableBookCopyCommandHandler(
    IAppDbContext context,
    ILogger<MakeAvilableBookCopyCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<MakeAvilableBookCopyCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MakeAvilableBookCopyCommand command, CancellationToken ct)
    {
        var bookCopy = await context.BookCopies
            .FirstOrDefaultAsync(bc => bc.Id == command.BookCopyId, ct);

        if (bookCopy is null)
        {
            logger.LogWarning(
                "Book copy {BookCopyId} not found for making available.",
                command.BookCopyId);

            return BookCopyApplicationErrors.NotFoundById;
        }

        var availableResult = bookCopy.MarkAsAvilable();

        if (availableResult.IsFailure)
            return availableResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCopyCachingConstants.BookCopyTag, ct);

        logger.LogInformation(
            "Book copy {BookCopyId} has been marked as available",
            bookCopy.Id);

        return Result.Updated;
    }
}
