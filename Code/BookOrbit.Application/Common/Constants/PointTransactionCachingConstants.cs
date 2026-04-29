
namespace BookOrbit.Application.Common.Constants;

public class PointTransactionCachingConstants
{
    public const string PointTransactionTag = "pointTransaction";

    public static string PointTransactionKey(Guid id) => $"pointTransaction:{id}";

    public static string PointTransactionListKey(GetPointTransactionsQuery query)
        =>
        $"pointtransaction:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
        $":studentid={query.StudentId?.ToString() ?? "-"}" +
        $":borrowingreviewid={query.BorrowingReviewId?.ToString() ?? "-"}" +
        $":reasons=[{string.Join(',', query.Reasons ?? [])}]";

    public const int ExpirationInMinutes = 10;
}
