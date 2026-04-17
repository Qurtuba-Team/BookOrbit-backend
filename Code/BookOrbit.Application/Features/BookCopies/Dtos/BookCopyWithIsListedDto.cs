namespace BookOrbit.Application.Features.BookCopies.Dtos;
public class BookCopyWithIsListedDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BookId { get; set; } = Guid.Empty;
    public Guid OwnerId { get; set; } = Guid.Empty;
    public BookCopyCondition Condition { get; set; }
    public BookCopyState State { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsListed { get; set; }

    public BookCopyWithIsListedDto(
    Guid id,
    Guid bookId,
    Guid ownerId,
    BookCopyCondition condition,
    BookCopyState state,
    string ownerName,
    string title,
    bool isListed)
    {
        Id = id;
        BookId = bookId;
        OwnerId = ownerId;
        Condition = condition;
        State = state;
        OwnerName = ownerName;
        Title = title;
        IsListed = isListed;
    }

    public BookCopyWithIsListedDto()
    {
    }
}