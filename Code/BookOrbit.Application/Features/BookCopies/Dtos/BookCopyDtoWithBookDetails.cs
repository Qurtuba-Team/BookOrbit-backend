namespace BookOrbit.Application.Features.BookCopies.Dtos;
public record BookCopyDtoWithBookDetails
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BookId {  get; set; } = Guid.Empty;
    public Guid OwnerId { get; set; } = Guid.Empty;
    public BookCopyCondition Condition { get; set; }
    public BookCopyState State { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public BookDto Book { get; set; }
    public bool IsListed { get; set; }

#pragma warning disable CS8618 // Non-nullable property must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [JsonConstructor]
    private BookCopyDtoWithBookDetails() { }

    private BookCopyDtoWithBookDetails(
        Guid id,
        Guid bookId,
        Guid ownerId,
        BookCopyCondition condition,
        BookCopyState state,
        string ownerName,
        BookDto book,
        bool isListed)
    {
        Id = id;
        BookId = bookId;
        OwnerId = ownerId;
        Condition = condition;
        State = state;
        OwnerName = ownerName;
        Book = book;
        IsListed = isListed;
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
            book,
            isListed);
}