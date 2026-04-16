namespace BookOrbit.Domain.BorrowingTransactions;

static public class BorrowingTransactionErrors
{
    private const string ClassName = nameof(BorrowingTransaction);

    #region General
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error BorrowingRequestIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BorrowingRequestId", "Borrowing Request Id");
    static public readonly Error LenderStudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "LenderStudentId", "Lender Student Id");
    static public readonly Error BorrowerStudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BorrowerStudentId", "Borrower Student Id");
    static public readonly Error BookCopyIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BookCopyId", "BookCopyId");
    static public readonly Error ExpectedReturnDateRequired = DomainCommonErrors.RequiredProp(ClassName, "ExpectedReturnDate", "Expected Return Date");
    static public readonly Error InvalidExpectedReturnDate = DomainCommonErrors.DateShouldBeInFuture(ClassName, "ExpectedReturnDate", "Expected Return Date");
    static public readonly Error LenderAndBorrowerCannotBeTheSame = DomainCommonErrors.CustomValidation(ClassName, "LenderAndBorrowerCannotBeTheSame", "Lender and borrower cannot be the same student.");
    static public readonly Error ReturnDateShouldBeAfterCreationDate = DomainCommonErrors.CustomValidation(ClassName, "ReturnDateShouldBeAfterCreaionDate", "Return date should be after creation date.");
    static public readonly Error ReturnDateCannotBeInTheFuture = DomainCommonErrors.DateCannotBeInFuture(ClassName, "ReturnDate", "Return Date");
    static public readonly Error CannotMarkOverdueWhileExpectedReturnDateNotPast = DomainCommonErrors.CustomValidation(ClassName, "CannotMarkOverdueWhileExcpectedReturnDateNotPast", "Cannot mark overdue while expected return date not past.");
    #endregion

    #region Logic
    public static Error InvalidStateTransition(BorrowingTransactionState currentState, BorrowingTransactionState newState) =>
     DomainCommonErrors.InvalidStateTransition(ClassName, currentState.ToString(), newState.ToString());

    #endregion
}
