namespace BookOrbit.Application.Features.LendingListings.Dtos;
public record LendingListRecordListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BookCopyId { get; set; } = Guid.Empty;
    public Guid OwnerId { get; set; } = Guid.Empty;
    public BookCopyCondition Condition { get; set; }
    public BookCopyState BookCopyState { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public LendingListRecordState State { get; set; }
    public int BorrowingDurationInDays { get; set; }
    public int Cost { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    [JsonConstructor]
    private LendingListRecordListItemDto() { }

    public LendingListRecordListItemDto(
        Guid id,
        Guid bookCopyId,
        Guid ownerId,
        BookCopyCondition condition,
        BookCopyState bookCopyState,
        string ownerName,
        string title,
        LendingListRecordState state,
        int borrowingDurationInDays,
        int cost,
        DateTimeOffset createdAt)
    {
        Id = id;
        BookCopyId = bookCopyId;
        OwnerId = ownerId;
        Condition = condition;
        BookCopyState = bookCopyState;
        OwnerName = ownerName;
        Title = title;
        State = state;
        BorrowingDurationInDays = borrowingDurationInDays;
        Cost = cost;
        CreatedAt = createdAt;
    }

    public static Expression<Func<LendingListRecord, LendingListRecordListItemDto>> Projection =>
        lr => new LendingListRecordListItemDto(
            lr.Id,
            lr.BookCopyId,
            lr.BookCopy!.Owner!.Id,
            lr.BookCopy!.Condition,
            lr.BookCopy!.State,
            lr.BookCopy!.Owner!.Name.Value,
            lr.BookCopy!.Book!.Title.Value,
            lr.State,
            lr.BorrowingDurationInDays,
            lr.Cost.Value,
            lr.CreatedAtUtc);
}