namespace BookOrbit.Application.Features.Books.Queries.GetBooks;
public record GetBooksQuery
    (
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    List<BookCategory>? Categories = null)
    : ICachedQuery<Result<PaginatedList<BookListItemDto>>>
{
    public string CacheKey => BookCachingConstants.BookListKey(this);

    public string[] Tags => [BookCachingConstants.BookTag];

    public TimeSpan Expiration =>TimeSpan.FromMinutes(BookCachingConstants.ExpirationInMinutes);
}