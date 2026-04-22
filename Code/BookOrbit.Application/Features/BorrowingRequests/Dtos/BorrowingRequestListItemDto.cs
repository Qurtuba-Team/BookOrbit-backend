namespace BookOrbit.Application.Features.BorrowingRequests.Dtos;
public record BorrowingRequestListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingStudentId { get; set; } = Guid.Empty;
    public Guid LendingRecordId { get; set; } = Guid.Empty;
    public Guid LendingStudentId { get; set; } = Guid.Empty;
    public string BorrowingStudentName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public Guid BookId { get; set; } = Guid.Empty;
    public Guid BookCopyId { get; set; } = Guid.Empty;
    public BorrowingRequestState State { get; set; }
    public DateTimeOffset? ExpirationDateUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }


    [JsonConstructor]
    private BorrowingRequestListItemDto() { }

    public BorrowingRequestListItemDto(
        Guid id,
        Guid borrowingStudentId,
        Guid lendingRecordId,
        Guid lendingStudentId,
        string borrowingStudentName,
        string bookTitle,
        Guid bookId,
        Guid bookCopyId,
        BorrowingRequestState state,
        DateTimeOffset? expirationDateUtc,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        BorrowingStudentId = borrowingStudentId;
        LendingRecordId = lendingRecordId;
        LendingStudentId = lendingStudentId;
        BorrowingStudentName = borrowingStudentName;
        BookTitle = bookTitle;
        BookId = bookId;
        BookCopyId = bookCopyId;
        State = state;
        ExpirationDateUtc = expirationDateUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static Expression<Func<BorrowingRequest, BorrowingRequestListItemDto>> Projection =>
        br => new BorrowingRequestListItemDto(
            br.Id,
            br.BorrowingStudentId,
            br.LendingRecordId,
            br.LendingRecord!.BookCopy!.OwnerId,
            br.BorrowingStudent!.Name.Value,
            br.LendingRecord!.BookCopy!.Book!.Title.Value,
            br.LendingRecord!.BookCopy!.BookId,
            br.LendingRecord!.BookCopyId,
            br.State,
            br.ExpirationDateUtc,
            br.CreatedAtUtc);
}
