namespace BookOrbit.Application.Features.BookCopies.Dtos;
public record BookCopyListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BookId { get; set; } = Guid.Empty;
    public Guid OwnerId { get; set; } = Guid.Empty;
    public BookCopyCondition Condition { get; set; }
    public BookCopyState State { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [JsonConstructor]
    private BookCopyListItemDto() { }

    private BookCopyListItemDto(
        Guid id,
        Guid bookId,
        Guid ownerId,
        BookCopyCondition condition,
        BookCopyState state,
        string ownerName,
        string title)
    {
        Id = id;
        BookId = bookId;
        OwnerId = ownerId;
        Condition = condition;
        State = state;
        OwnerName = ownerName;
        Title = title;
    }

    public static Expression<Func<BookCopy, BookCopyListItemDto>> Projection =>
        b => new BookCopyListItemDto(
            b.Id,
            b.BookId,
            b.OwnerId,
            b.Condition,
            b.State,
            b.Owner!.Name.Value,
            b.Book!.Title.Value);
}
