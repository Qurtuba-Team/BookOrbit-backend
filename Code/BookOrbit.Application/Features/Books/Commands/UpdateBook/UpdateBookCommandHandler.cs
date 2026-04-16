using BookOrbit.Domain.Students;

namespace BookOrbit.Application.Features.Books.Commands.UpdateBook;
public class UpdateBookCommandHandler(
    ILogger<UpdateBookCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<UpdateBookCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateBookCommand command, CancellationToken ct)
    {
        var book = await context.Books
            .FirstOrDefaultAsync(b => b.Id == command.Id, ct);

        if (book is null)
        {
            logger.LogWarning("Book {BookId} not found for update.", command.Id);

            return BookApplicationErrors.NotFoundById;
        }

        var titleCreationResult = BookTitle.Create(command.Title);

        if (titleCreationResult.IsFailure)
            return titleCreationResult.Errors;

        var updateResult = book.Update(titleCreationResult.Value);

        if (updateResult.IsFailure)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCachingConstants.BookTag, ct);

        logger.LogInformation(
            "Book updated successfully. Id: {BookId}, Title: {Title}",
            book.Id,
            book.Title.Value);

        return Result.Updated;

    }
}