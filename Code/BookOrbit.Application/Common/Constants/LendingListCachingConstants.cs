namespace BookOrbit.Application.Common.Constants;
public class LendingListCachingConstants
{
    public const string LendingListTag = "lendingList";

    public static string LendingListKey(Guid id) => $"lendingList:{id}";

    //public static string LendingListListKey(GetLendingListsQuery query)
    //    =>
    //    $"students:p={query.Page}:ps={query.PageSize}" +
    //    $":st={query.SearchTerm ?? "-"}" +
    //    $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
    //    $":states=[{string.Join(',', query.Categories ?? [])}]";

    public const int ExpirationInMinutes = 10;
}
