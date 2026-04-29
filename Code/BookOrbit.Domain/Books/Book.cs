namespace BookOrbit.Domain.Books;
public class Book : AuditableEntity
{
    public BookTitle Title { get; private set; } = null!;
    public ISBN ISBN { get; } = null!;
    public BookPublisher Publisher { get; } = null!;
    public BookCategory Category { get; }
    public BookAuthor Author { get; } = null!;
    public string CoverImageFileName { get; private set; } = null!;
    public BookStatus Status { get; private set; }

    private Book() { }

    private Book(
        Guid id,
        BookTitle title,
        ISBN isbn,
        BookPublisher publisher,
        BookCategory category,
        BookAuthor author,
        string coverImageFileName) : base(id)
    {
        Title = title;
        ISBN = isbn;
        Publisher = publisher;
        Category = category;
        Author = author;
        CoverImageFileName = coverImageFileName;
        Status = BookStatus.Pending;
    }

    public static Result<Book> Create(
        Guid id,
        BookTitle title,
        ISBN isbn,
        BookPublisher publisher,
        BookCategory category,
        BookAuthor author,
        string coverImageFileName)
    {
        if (id == Guid.Empty)
            return BookErrors.IdRequired;

        if (title is null)
            return BookErrors.TitleRequired;

        if (isbn is null)
            return BookErrors.ISBNRequired;

        if (publisher is null)
            return BookErrors.PublisherRequired;

        if (author is null)
            return BookErrors.AuthorRequired;



        if (string.IsNullOrWhiteSpace(coverImageFileName))
            return BookErrors.CoverImagePhotoRequired;


        return new Book(
            id,
            title,
            isbn,
            publisher,
            category,
            author,
            coverImageFileName);
    }


    public Result<Updated> Update(
        BookTitle title,
        string coverImageFileName)
    {
        if (title is null)
            return BookErrors.TitleRequired;

        if(string.IsNullOrWhiteSpace(coverImageFileName))
            return BookErrors.CoverImagePhotoRequired;

        Title = title;
        CoverImageFileName = coverImageFileName;

        return Result.Updated;
    }

    private bool CanTransitionToStatus(BookStatus newStatus)
    {
        return Status switch
        {
            BookStatus.Pending => newStatus is BookStatus.Available or BookStatus.Rejected,
            BookStatus.Available => false,
            BookStatus.Rejected => false,
            _ => false
        };
    }

    private Result<Updated> UpdateStatus(BookStatus newStatus)
    {
        if (!CanTransitionToStatus(newStatus))
            return BookErrors.InvalidStatusTransition(Status, newStatus);

        Status = newStatus;
        return Result.Updated;
    }

    public Result<Updated> MarkAsAvailable()
        => UpdateStatus(BookStatus.Available);

    public Result<Updated> MarkAsRejected()
        => UpdateStatus(BookStatus.Rejected);
}
