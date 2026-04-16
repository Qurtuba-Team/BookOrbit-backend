namespace BookOrbit.Application.Features.Books.Queries.GetBookById;
public record GetBookByIdQuery(
    Guid BookId) : ICachedQuery<Result<BookDto>>
{
    public string CacheKey => BookCachingConstants.BookKey(BookId);

    public string[] Tags => [BookCachingConstants.BookTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BookCachingConstants.ExpirationInMinutes);
}