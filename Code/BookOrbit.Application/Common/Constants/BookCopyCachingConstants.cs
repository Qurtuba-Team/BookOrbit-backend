namespace BookOrbit.Application.Common.Constants;

static public class BookCopyCachingConstants
{
    public const string BookCopyTag = "bookCopy";
    public static string BookCopyKey(Guid id) => $"bookCopy:{id}";

    public static string BookCoopyListKey(GetBookCopiesQuery query)
        =>
        $"bookcopies:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection??"-"}"+
        $":states=[{string.Join(',', query.States ?? [])}]"+
        $":conditions=[{string.Join(',', query.Conditions ?? [])}]"+
        $":bookid={query.BookId?.ToString()??"-"}"+
        $":ownerid={query.OwnerId?.ToString() ?? "-"}";

    public const int ExpirationInMinutes = 10;
}

