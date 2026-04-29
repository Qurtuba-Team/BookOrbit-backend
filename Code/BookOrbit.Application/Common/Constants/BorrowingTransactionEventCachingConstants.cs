
namespace BookOrbit.Application.Common.Constants;

public class BorrowingTransactionEventCachingConstants
{
    public const string BorrowingTransactionEventTag = "borrowingTransactionEvent";

    public static string BorrowingTransactionEventKey(Guid id) => $"borrowingtransactionevent:{id}";

    public static string BorrowingTransactionEventListKey(GetBorrowingTransactionEventsQuery query)
        =>
        $"borrowingtransactionevent:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
        $":states=[{string.Join(',', query.States ?? [])}]" +
        $":borrowingtransactionid={query.BorrowingTransactionId?.ToString() ?? "-"}";

    public const int ExpirationInMinutes = 10;
}
