namespace BookOrbit.Application.Features.BookCopies.Commands.CreateBookCopy;
public class CreateBookCopyCommandHandler(
    ILogger<CreateBookCopyCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IRouteService routeService) : IRequestHandler<CreateBookCopyCommand, Result<BookCopyDtoWithBookDetails>>

{
    public async Task<Result<BookCopyDtoWithBookDetails>> Handle(CreateBookCopyCommand command, CancellationToken ct)
    {
        var book = await context.Books.FirstOrDefaultAsync(b => b.Id == command.BookId, ct);

        if (book is null)
        {
            logger.LogWarning("Book not found. BookId: {BookId}", command.BookId);

            return BookApplicationErrors.NotFoundById;
        }

        if (book.Status is not BookStatus.Available)
        {
            logger.LogWarning("Book is not available. BookId: {BookId}", command.BookId);
            return BookApplicationErrors.BookIsNotAvailable;
        }

        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.OwnerId, ct);

        if (student is null)
        {
            logger.LogWarning("Student not found. StudentId: {StudentId}", command.OwnerId);

            return StudentApplicationErrors.NotFoundById;
        }

        if(student.State is not StudentState.Active)
        {
            logger.LogWarning("Student is not active. StudentId: {StudentId}", command.OwnerId);

            return StudentApplicationErrors.StateIsNotActive;
        }


        var createdBookCopyResult = BookCopy.Create(
            id: Guid.NewGuid(),
            ownerId: command.OwnerId,
            bookId: command.BookId,
            condition: command.Condition);

        if (createdBookCopyResult.IsFailure)
        {
            logger.LogWarning("Book copy creation failed. Errors : {Errors}", string.Join(',', createdBookCopyResult.Errors));
            return createdBookCopyResult.Errors;
        }

        context.BookCopies.Add(createdBookCopyResult.Value);

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCopyCachingConstants.BookCopyTag, ct);

        logger.LogInformation("Book copy created successfully with ID: {BookCopyId}", createdBookCopyResult.Value.Id);

        bool isListed = await context.LendingListRecords.AnyAsync(l => 
        l.BookCopyId == createdBookCopyResult.Value.Id &&
                (l.State == LendingListRecordState.Available
                ||
                l.State == LendingListRecordState.Reserved
                ||
                l.State == LendingListRecordState.Borrowed), ct);

        string bookCoverImageUrl = routeService.GetBookCoverImageRoute(book.CoverImageFileName);

        return BookCopyDtoWithBookDetails.FromEntity(
            createdBookCopyResult.Value,
            student.Name.Value,
            BookDto.FromEntity(book, bookCoverImageUrl),
            isListed);
    }
}