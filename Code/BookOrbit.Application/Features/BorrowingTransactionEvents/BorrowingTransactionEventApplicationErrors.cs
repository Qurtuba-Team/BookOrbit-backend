using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

namespace BookOrbit.Application.Features.BorrowingTransactionEvents;

public static class BorrowingTransactionEventApplicationErrors
{
    private const string ClassName = nameof(BorrowingTransactionEvent);

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
}
