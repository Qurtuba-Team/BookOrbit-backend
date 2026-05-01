
using BookOrbit.Domain.BorrowingTransactions.DomainEvents;

namespace BookOrbit.Domain.BorrowingTransactions;

public class BorrowingTransaction : AuditableEntity
{
    public Guid BorrowingRequestId { get; }
    public Guid LenderStudentId { get; }
    public Guid BorrowerStudentId { get; }
    public Guid BookCopyId { get; }
    public BorrowingTransactionState State { get; private set; }
    public DateTimeOffset ExpectedReturnDate { get; }
    public DateTimeOffset? ActualReturnDate { get; private set; }


    public BorrowingRequest? BorrowingRequest { get; private set; }
    public Student? LenderStudent { get; private set; }
    public Student? BorrowerStudent { get; private set; }
    public BookCopy? BookCopy { get; private set; }

    private BorrowingTransaction() { }

    private BorrowingTransaction(
        Guid id,
        Guid borrowingRequestId,
        Guid lenderStudentId,
        Guid borrowerStudentId,
        Guid bookCopyId,
        DateTimeOffset expectedReturnDate) : base(id)
    {
        BorrowingRequestId = borrowingRequestId;
        LenderStudentId = lenderStudentId;
        BorrowerStudentId = borrowerStudentId;
        BookCopyId = bookCopyId;
        State = BorrowingTransactionState.Borrowed;
        ExpectedReturnDate = expectedReturnDate;
        ActualReturnDate = null;
    }


    public static Result<BorrowingTransaction> Create(
        Guid id,
        Guid borrowingRequestId,
        Guid lenderStudentId,
        Guid borrowerStudentId,
        Guid bookCopyId,
        DateTimeOffset expectedReturnDate,
        DateTimeOffset currentTime // This parameter is added to make the method more testable, as it allows us to control the current time during testing. 
        )
    {
        if (id == Guid.Empty)
            return BorrowingTransactionErrors.IdRequired;

        if (borrowingRequestId == Guid.Empty)
            return BorrowingTransactionErrors.BorrowingRequestIdRequired;

        if (lenderStudentId == Guid.Empty)
            return BorrowingTransactionErrors.LenderStudentIdRequired;

        if (borrowerStudentId == Guid.Empty)
            return BorrowingTransactionErrors.BorrowerStudentIdRequired;

        if (bookCopyId == Guid.Empty)
            return BorrowingTransactionErrors.BookCopyIdRequired;

        if (expectedReturnDate <= currentTime)
            return BorrowingTransactionErrors.InvalidExpectedReturnDate;

        if (lenderStudentId == borrowerStudentId)
            return BorrowingTransactionErrors.LenderAndBorrowerCannotBeTheSame;


        var transaction = new BorrowingTransaction(
        id,
        borrowingRequestId,
        lenderStudentId,
        borrowerStudentId,
        bookCopyId,
        expectedReturnDate);
        
        transaction.AddDomainEvent(new BorrowingTransactionStateChangedEvent(transaction.Id, transaction.State));
        
        return transaction;
    }

    private bool CanTransitionToState(BorrowingTransactionState newState)
    {
        return State switch
        {
            BorrowingTransactionState.Borrowed => newState is BorrowingTransactionState.Returned or BorrowingTransactionState.Overdue or BorrowingTransactionState.Lost,
            BorrowingTransactionState.Overdue => newState is BorrowingTransactionState.Lost or BorrowingTransactionState.Returned,
            BorrowingTransactionState.Returned => false,
            BorrowingTransactionState.Lost => newState is BorrowingTransactionState.Returned,
            _ => false
        };
    }

    private Result<Updated> UpdateState(BorrowingTransactionState newState)
    {
        if (!CanTransitionToState(newState))
            return BorrowingTransactionErrors.InvalidStateTransition(State, newState);

        State = newState;
        
        AddDomainEvent(new BorrowingTransactionStateChangedEvent(Id, State));

        return Result.Updated;
    }

    public Result<Updated> ReturnBookCopy(DateTimeOffset returnDate , DateTimeOffset currentTime)
    {
        if(returnDate <= CreatedAtUtc)
            return BorrowingTransactionErrors.ReturnDateShouldBeAfterCreationDate;

        if(returnDate > currentTime)
            return BorrowingTransactionErrors.ReturnDateCannotBeInTheFuture;

        BorrowingTransactionState ToState = BorrowingTransactionState.Returned;

        if (returnDate > ExpectedReturnDate)
            ToState = BorrowingTransactionState.Overdue;

        var result = UpdateState(ToState);

        if (result.IsFailure)
            return result;
                
        ActualReturnDate = returnDate;
        return Result.Updated;
    }

    public Result<Updated> MarkAsOverdue(DateTimeOffset currentTime)
    {
        if (ExpectedReturnDate >= currentTime)
            return BorrowingTransactionErrors.CannotMarkOverdueWhileExpectedReturnDateNotPast;

        return UpdateState(BorrowingTransactionState.Overdue);
    }

    public Result<Updated> MarkAsLost()
    {
        return UpdateState(BorrowingTransactionState.Lost);
    }

    public Result<Updated> MarkAsBorrowed()
    {
        return UpdateState(BorrowingTransactionState.Borrowed);
    }

}