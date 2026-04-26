namespace BookOrbit.Application.Features.BorrowingTransactions.Dtos;
public record BorrowingTransactionDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingRequestId { get; set; } = Guid.Empty;
    public Guid LenderStudentId { get; set; } = Guid.Empty;
    public Guid BorrowerStudentId { get; set; } = Guid.Empty;
    public Guid BookCopyId { get; set; } = Guid.Empty;
    public BorrowingTransactionState State { get; set; }
    public DateTimeOffset ExpectedReturnDate { get; set; }
    public DateTimeOffset? ActualReturnDate { get; set; }

    [JsonConstructor]
    private BorrowingTransactionDto() { }

    private BorrowingTransactionDto(
        Guid id,
        Guid borrowingRequestId,
        Guid lenderStudentId,
        Guid borrowerStudentId,
        Guid bookCopyId,
        BorrowingTransactionState state,
        DateTimeOffset expectedReturnDate,
        DateTimeOffset? actualReturnDate)
    {
        Id = id;
        BorrowingRequestId = borrowingRequestId;
        LenderStudentId = lenderStudentId;
        BorrowerStudentId = borrowerStudentId;
        BookCopyId = bookCopyId;
        State = state;
        ExpectedReturnDate = expectedReturnDate;
        ActualReturnDate = actualReturnDate;
    }

    public static BorrowingTransactionDto FromEntity(BorrowingTransaction transaction)
        => new(
            transaction.Id,
            transaction.BorrowingRequestId,
            transaction.LenderStudentId,
            transaction.BorrowerStudentId,
            transaction.BookCopyId,
            transaction.State,
            transaction.ExpectedReturnDate,
            transaction.ActualReturnDate);
}
