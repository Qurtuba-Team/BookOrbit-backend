
namespace BookOrbit.Application.Common.Constants;
public class BorrowingTransactionCachingConstants
{
    public const string BorrowingTransactionTag = "borrowingTransaction";

    public static string BorrowingTransactionKey(Guid id) => $"borrowingTransaction:{id}";

    public static string BorrowingTransactionListKey(GetBorrowingTransactionsQuery query)
        =>
        $"borrowingtransaction:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
        $":states=[{string.Join(',', query.States ?? [])}]" +
        $":borrowerid={query.BorrowerStudentId?.ToString() ?? "-"}" +
        $":lenderid={query.LenderStudentId?.ToString() ?? "-"}" +
        $":bookcopyid={query.BookCopyId?.ToString() ?? "-"}" +
        $":borrowingrequestid={query.BorrowingRequestId?.ToString() ?? "-"}";

    public const int ExpirationInMinutes = 10;
}
