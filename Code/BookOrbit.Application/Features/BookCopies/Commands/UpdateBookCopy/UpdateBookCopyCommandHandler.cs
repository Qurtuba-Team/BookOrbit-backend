namespace BookOrbit.Application.Features.BookCopies.Commands.UpdateBookCopy;
public class UpdateBookCopyCommandHandler(
    ILogger<UpdateBookCopyCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<UpdateBookCopyCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateBookCopyCommand command, CancellationToken ct)
    {
        var bookCopy = await context.BookCopies.FirstOrDefaultAsync(
            b => b.Id == command.Id,ct);

        if(bookCopy is null)
        {
            logger.LogWarning("Book Copy {BookCopyId} not found for update.", command.Id);

            return BookCopyApplicationErrors.NotFoundById;
        }

        var updateResult = bookCopy.Update(command.Condition);

        if (updateResult.IsFailure)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCopyCachingConstants.BookCopyTag, ct);

        logger.LogInformation(
            "BookCopy updated successfully. Id: {BookCopyId}, Condition: {Condition}",
            bookCopy.Id,
            bookCopy.Condition);

        return Result.Updated;
    }
}