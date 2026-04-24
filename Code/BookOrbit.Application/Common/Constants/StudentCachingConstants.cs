
namespace BookOrbit.Application.Common.Constants;

static public class StudentCachingConstants
{
    public const string StudentTag = "student";
    public static string StudentKey(Guid id) => $"student:{id}";
    public static string StudentProfileKey(Guid id) => $"student:profile:{id}";
    public static string StudentContactInformationKeyByLendingListId(Guid id) => $"student:contact:lendinglist:{id}";

    public static string StudentListKey(GetStudentsQuery query)
        =>
        $"students:p={query.Page}:ps={query.PageSize}" +
        $":st={query.SearchTerm ?? "-"}" +
        $":sort={query.SortColumn}:{query.SortDirection??"-"}"+
        $":states=[{string.Join(',', query.States ?? [])}]"+
        $":emailconfirmed={query.EmailConfirmed.ToString()??"-"}";

    public const int ExpirationInMinutes = 10;
}

