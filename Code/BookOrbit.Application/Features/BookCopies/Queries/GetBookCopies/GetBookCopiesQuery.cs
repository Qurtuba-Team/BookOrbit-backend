namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopies;
public record GetBookCopiesQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? BookId = null,
    Guid? OwnerId = null,
    List<BookCopyCondition>? Conditions = null,
    List<BookCopyState>? States = null) : ICachedQuery<Result<PaginatedList<BookCopyListItemDto>>>
{
    public string CacheKey => BookCopyCachingConstants.BookCoopyListKey(this);

    public string[] Tags => [BookCopyCachingConstants.BookCopyTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BookCopyCachingConstants.ExpirationInMinutes);
}