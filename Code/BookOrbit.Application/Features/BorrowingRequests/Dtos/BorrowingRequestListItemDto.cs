namespace BookOrbit.Application.Features.BorrowingRequests.Dtos;
public record BorrowingRequestListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingStudentId { get; set; } = Guid.Empty;
    public Guid LendingRecordId { get; set; } = Guid.Empty;
    public string BorrowingStudentName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public BorrowingRequestState State { get; set; }
    public DateTimeOffset? ExpirationDateUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private BorrowingRequestListItemDto() { }

    public BorrowingRequestListItemDto(
        Guid id,
        Guid borrowingStudentId,
        Guid lendingRecordId,
        string borrowingStudentName,
        string bookTitle,
        BorrowingRequestState state,
        DateTimeOffset? expirationDateUtc,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        BorrowingStudentId = borrowingStudentId;
        LendingRecordId = lendingRecordId;
        BorrowingStudentName = borrowingStudentName;
        BookTitle = bookTitle;
        State = state;
        ExpirationDateUtc = expirationDateUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static Expression<Func<BorrowingRequest, BorrowingRequestListItemDto>> Projection =>
        br => new BorrowingRequestListItemDto(
            br.Id,
            br.BorrowingStudentId,
            br.LendingRecordId,
            br.BorrowingStudent!.Name.Value,
            br.LendingRecord!.BookCopy!.Book!.Title.Value,
            br.State,
            br.ExpirationDateUtc,
            br.CreatedAtUtc);
}
