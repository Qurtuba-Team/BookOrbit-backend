
namespace BookOrbit.Application.Common.Constants;

public class BorrowingReviewCachingConstants
{
    public const string BorrowingReviewTag = "borrowingReview";

    public static string BorrowingReviewKey(Guid id) => $"borrowingreview:{id}";

    public static string BorrowingReviewListKey(GetBorrowingReviewsQuery query)
        =>
        $"borrowingreview:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
        $":reviewerid={query.ReviewerStudentId?.ToString() ?? "-"}" +
        $":reviewedid={query.ReviewedStudentId?.ToString() ?? "-"}" +
        $":borrowingtransactionid={query.BorrowingTransactionId?.ToString() ?? "-"}";

    public const int ExpirationInMinutes = 10;
}
