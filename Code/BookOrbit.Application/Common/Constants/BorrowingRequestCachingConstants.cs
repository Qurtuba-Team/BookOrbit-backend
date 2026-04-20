namespace BookOrbit.Application.Common.Constants;
public class BorrowingRequestCachingConstants
{
    public const string BorrowingRequestTag = "borrowingRequest";

    public static string BorrowingRequestKey(Guid id) => $"borrowingRequest:{id}";

    public static string BorrowingRequestListKey(GetBorrowingRequestsQuery query)
        =>
        $"borrowingrequest:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
        $":states=[{string.Join(',', query.States ?? [])}]" +
        $":borrowerid={query.BorrowingStudentId?.ToString() ?? "-"}" +
        $":lendingrecordid={query.LendingRecordId?.ToString() ?? "-"}";

    public const int ExpirationInMinutes = 10;
}
