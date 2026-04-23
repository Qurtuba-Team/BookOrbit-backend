
namespace BookOrbit.Application.Common.Constants;
public class BookCachingConstants
{
    public const string BookTag = "book";

    public static string BookKey(Guid id) => $"book:{id}";

    public static string BookListKey(GetBooksQuery query)
        =>
        $"students:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
        $":categories=[{string.Join(',', query.Categories ?? [])}]" +
        $":statuses=[{string.Join(',', query.Statuses ?? [])}]";

    public const int ExpirationInMinutes = 10;

}