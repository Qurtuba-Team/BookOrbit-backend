
namespace BookOrbit.Application.Features.PointTransactions;

static public class PointTransactionApplicationErrors
{
    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(PointTransactionErrors.ClassName, "Id", "Id");
}
