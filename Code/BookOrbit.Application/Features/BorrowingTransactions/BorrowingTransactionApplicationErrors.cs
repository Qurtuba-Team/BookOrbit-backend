namespace BookOrbit.Application.Features.BorrowingTransactions;
public static class BorrowingTransactionApplicationErrors
{
    private const string ClassName = nameof(BorrowingTransaction);

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundProp(
        ClassName,
        "BorrowingTransaction",
        "Borrowing Transaction");

    public static readonly Error StudentNotBorrower = ApplicationCommonErrors.CustomUnauthorized(
        ClassName,
        "StudentNotBorrower",
        "Student is not the borrower for this transaction.");
    public static readonly Error InvalidState = ApplicationCommonErrors.CustomValidation(
        ClassName,
        "InvalidState",
        "The borrowing transaction is not in a valid state for this operation.");
}
