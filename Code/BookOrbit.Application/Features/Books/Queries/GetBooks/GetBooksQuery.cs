namespace BookOrbit.Application.Features.Books.Queries.GetBooks;
public record GetBooksQuery
    (
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    List<BookCategory>? Categories = null,
    List<BookStatus>? Statuses = null)
    : IPagedQuery<BookListItemDto>
{
    public string CacheKey => BookCachingConstants.BookListKey(this);

    public string[] Tags => [BookCachingConstants.BookTag];

    public TimeSpan Expiration =>TimeSpan.FromMinutes(BookCachingConstants.ExpirationInMinutes);
}