namespace BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

public class BorrowingTransactionEvent : AuditableEntity
{
    public Guid BorrowingTransactionId { get; }
    public BorrowingTransactionState State { get; }

    private BorrowingTransactionEvent() { }


    private BorrowingTransactionEvent(
        Guid id,
        Guid borrowingTransactionId,
        BorrowingTransactionState state) : base(id)
    {
        BorrowingTransactionId = borrowingTransactionId;
        State = state;
    }

    public static Result<BorrowingTransactionEvent> Create(
        Guid id,
        Guid borrowingTransactionId,
        BorrowingTransactionState state)
    {
        if (id == Guid.Empty)
            return BorrowingTransactionEventErrors.IdRequired;

        if (borrowingTransactionId == Guid.Empty)
            return BorrowingTransactionEventErrors.BorrowingTransactionIdRequired;

        if(!Enum.IsDefined(state))
            return BorrowingTransactionEventErrors.InvalidState;

        return new BorrowingTransactionEvent(
            id,
            borrowingTransactionId,
            state);
    }
}