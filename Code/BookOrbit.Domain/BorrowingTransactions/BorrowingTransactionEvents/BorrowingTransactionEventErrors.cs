namespace BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

static public class BorrowingTransactionEventErrors
{
    private const string ClassName = nameof(BorrowingTransactionEvent);
    static public readonly Error BorrowingTransactionIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BorrowingTransactionId", "Borrowing Transaction Id");
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error InvalidState = DomainCommonErrors.InvalidProp(ClassName, "State","State","Invalid state value");

}
