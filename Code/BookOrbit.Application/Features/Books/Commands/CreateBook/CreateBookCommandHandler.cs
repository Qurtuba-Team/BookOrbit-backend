namespace BookOrbit.Application.Features.Books.Commands.CreateBook;
public class CreateBookCommandHandler(
    ILogger<CreateBookCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<CreateBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(CreateBookCommand command, CancellationToken ct)
    {
        if (command.Category is BookCategory.None)
            return BookErrors.CategoryRequired;


        var titleCreationResult = BookTitle.Create(command.Title);
        if (titleCreationResult.IsFailure)
            return titleCreationResult.Errors;


        var isbnCreationResult = ISBN.Create(command.ISBN);
        if (isbnCreationResult.IsFailure)
            return isbnCreationResult.Errors;

        var publisherCreationResult = BookPublisher.Create(command.Publisher);
        if (publisherCreationResult.IsFailure)
            return publisherCreationResult.Errors;

        
        var authorCreationResult = BookAuthor.Create(command.Author);
        if (authorCreationResult.IsFailure)
            return authorCreationResult.Errors;


        var isIsbnExists = await context.Books.AnyAsync(b => b.ISBN.Value == isbnCreationResult.Value.Value,ct);

        if(isIsbnExists)
        {
            logger.LogWarning(
                "Book creation failed. Reason: {Reason} , {Value}",
                "ISBN Exists",
                isbnCreationResult.Value);

            return BookApplicationErrors.IsbnAlreadyExists;
        }


        var createdBookResult = Book.Create(
            id:Guid.NewGuid(),
            title: titleCreationResult.Value,
            isbn: isbnCreationResult.Value,
            publisher: publisherCreationResult.Value,
            category:command.Category,
            author: authorCreationResult.Value,
            coverImageFileName:command.CoverImageFileName);


        if (createdBookResult.IsFailure)
        {
            logger.LogWarning("Book creation failed. Errors : {Errors}", string.Join(',', createdBookResult.Errors));
            return createdBookResult.Errors;
        }

        context.Books.Add(createdBookResult.Value);

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BookCachingConstants.BookTag, ct);

        logger.LogInformation("Book created successfully with ID: {BookId}", createdBookResult.Value.Id);

        return BookDto.FromEntity(createdBookResult.Value);
    }
}