namespace BookOrbit.Domain.BorrowingTransactions.DomainEvents;
public class BorrowingTransactionBookCopyReturnedEvent(Guid borrowingStudentId,Guid borrowingTransactionId, Guid bookCopyId, BorrowingTransactionState state) : DomainEvent
{
    public Guid BorrowingStudentId { get; } = borrowingStudentId;
    public Guid BorrowingTransactionId { get; } = borrowingTransactionId;
    public Guid BookCopyId { get; } = bookCopyId;
    public BorrowingTransactionState State { get; } = state;
}