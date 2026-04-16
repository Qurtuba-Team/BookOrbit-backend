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

    [JsonConstructor]
    private BookCopyDtoWithBookDetails() { }

    private BookCopyDtoWithBookDetails(
        Guid id, 
        Guid bookId,
        Guid ownerId,
        BookCopyCondition condition,
        BookCopyState state,
        string ownerName,
        BookDto book)
    {
        Id = id;
        BookId = bookId;
        OwnerId = ownerId;
        Condition = condition;
        State = state;
        OwnerName = ownerName;
        Book = book;
    }

    static public BookCopyDtoWithBookDetails FromEntity(
        BookCopy bookCopy,
        string ownerName,
        BookDto book)
        =>
        new(
            bookCopy.Id,
            bookCopy.BookId,
            bookCopy.OwnerId,
            bookCopy.Condition,
            bookCopy.State,
            ownerName,
            book);
}