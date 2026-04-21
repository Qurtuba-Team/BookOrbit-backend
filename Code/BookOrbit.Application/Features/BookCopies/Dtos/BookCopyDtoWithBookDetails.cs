namespace BookOrbit.Application.Features.BookCopies.Dtos;
public record BookCopyDtoWithBookDetails
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BookId {  get; set; } = Guid.Empty;
    public Guid OwnerId { get; set; } = Guid.Empty;
    public BookCopyCondition Condition { get; set; }
    public BookCopyState State { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public string Author { get; set; } = string.Empty;
    public bool IsListed { get; set; }
    public string BookCoverImageUrl { get; set; } = string.Empty;

#pragma warning disable CS8618 // Non-nullable property must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [JsonConstructor]
    private BookCopyDtoWithBookDetails() { }

    public BookCopyDtoWithBookDetails(Guid id,
                                      Guid bookId,
                                      Guid ownerId,
                                      BookCopyCondition condition,
                                      BookCopyState state,
                                      string ownerName,
                                      string title,
                                      string iSBN,
                                      string publisher,
                                      BookCategory category,
                                      string author,
                                      bool isListed,
                                      string bookCoverImageUrl)
    {
        Id = id;
        BookId = bookId;
        OwnerId = ownerId;
        Condition = condition;
        State = state;
        OwnerName = ownerName;
        Title = title;
        ISBN = iSBN;
        Publisher = publisher;
        Category = category;
        Author = author;
        IsListed = isListed;
        BookCoverImageUrl = bookCoverImageUrl;
    }

    static public BookCopyDtoWithBookDetails FromEntity(
        BookCopy bookCopy,
        string ownerName,
        BookDto book,
        bool isListed)
        =>
        new(
            bookCopy.Id,
            bookCopy.BookId,
            bookCopy.OwnerId,
            bookCopy.Condition,
            bookCopy.State,
            ownerName,
            book.Title,
            book.ISBN,
            book.Publisher,
            book.Category,
            book.Author,
            isListed,
            book.BookCoverImageUrl);
}