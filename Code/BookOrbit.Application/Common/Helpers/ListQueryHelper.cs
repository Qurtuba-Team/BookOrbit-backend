namespace BookOrbit.Application.Common.Helpers;
public static class ListQueryHelper
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize) where T : class
    {
        return query.Skip((page - 1) * pageSize)
              .Take(pageSize);
    }
}