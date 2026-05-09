
namespace BookOrbit.Application.Features.BorrowingTransactionEvents;

public static class BorrowingTransactionEventApplicationErrors
{
    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(BorrowingTransactionEventErrors.ClassName, "Id", "Id");
}
