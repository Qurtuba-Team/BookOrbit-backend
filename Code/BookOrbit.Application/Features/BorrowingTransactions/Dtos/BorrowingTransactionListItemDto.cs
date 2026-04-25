namespace BookOrbit.Application.Features.BorrowingTransactions.Dtos;
public record BorrowingTransactionListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingRequestId { get; set; } = Guid.Empty;
    public Guid LenderStudentId { get; set; } = Guid.Empty;
    public Guid BorrowerStudentId { get; set; } = Guid.Empty;
    public Guid BookCopyId { get; set; } = Guid.Empty;
    public string LenderStudentName { get; set; } = string.Empty;
    public string BorrowerStudentName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public BorrowingTransactionState State { get; set; }
    public DateTimeOffset ExpectedReturnDate { get; set; }
    public DateTimeOffset? ActualReturnDate { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private BorrowingTransactionListItemDto() { }

    public BorrowingTransactionListItemDto(
        Guid id,
        Guid borrowingRequestId,
        Guid lenderStudentId,
        Guid borrowerStudentId,
        Guid bookCopyId,
        string lenderStudentName,
        string borrowerStudentName,
        string bookTitle,
        BorrowingTransactionState state,
        DateTimeOffset expectedReturnDate,
        DateTimeOffset? actualReturnDate,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        BorrowingRequestId = borrowingRequestId;
        LenderStudentId = lenderStudentId;
        BorrowerStudentId = borrowerStudentId;
        BookCopyId = bookCopyId;
        LenderStudentName = lenderStudentName;
        BorrowerStudentName = borrowerStudentName;
        BookTitle = bookTitle;
        State = state;
        ExpectedReturnDate = expectedReturnDate;
        ActualReturnDate = actualReturnDate;
        CreatedAtUtc = createdAtUtc;
    }

    public static Expression<Func<BorrowingTransaction, BorrowingTransactionListItemDto>> Projection =>
        bt => new BorrowingTransactionListItemDto(
            bt.Id,
            bt.BorrowingRequestId,
            bt.LenderStudentId,
            bt.BorrowerStudentId,
            bt.BookCopyId,
            bt.LenderStudent.Name.Value,
            bt.BorrowerStudent.Name.Value,
            bt.BookCopy.Book!.Title.Value,
            bt.State,
            bt.ExpectedReturnDate,
            bt.ActualReturnDate,
            bt.CreatedAtUtc);
}
