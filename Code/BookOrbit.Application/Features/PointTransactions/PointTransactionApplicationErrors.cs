using BookOrbit.Domain.PointTransactions;

namespace BookOrbit.Application.Features.PointTransactions;

static public class PointTransactionApplicationErrors
{
    private const string ClassName = nameof(PointTransaction);

    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
}
