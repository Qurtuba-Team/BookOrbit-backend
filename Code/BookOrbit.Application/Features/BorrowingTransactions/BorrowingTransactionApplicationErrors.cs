namespace BookOrbit.Application.Features.BorrowingTransactions;
public static class BorrowingTransactionApplicationErrors
{
    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundProp(
        BorrowingTransactionErrors.ClassName,
        "BorrowingTransaction",
        "Borrowing Transaction");

    public static readonly Error StudentNotBorrower = ApplicationCommonErrors.CustomUnauthorized(
        BorrowingTransactionErrors.ClassName,
        "StudentNotBorrower",
        "Student is not the borrower for this transaction.");
    public static readonly Error InvalidState = ApplicationCommonErrors.CustomValidation(
        BorrowingTransactionErrors.ClassName,
        "InvalidState",
        "The borrowing transaction is not in a valid state for this operation.");
}
