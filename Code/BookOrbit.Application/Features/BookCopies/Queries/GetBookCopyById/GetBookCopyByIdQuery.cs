namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopyById;
public record GetBookCopyByIdQuery(
    Guid BookCopyId) : ICachedQuery<Result<BookCopyDtoWithBookDetails>>
{
    public string CacheKey => BookCopyCachingConstants.BookCopyKey(BookCopyId);

    public string[] Tags => [BookCopyCachingConstants.BookCopyTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BookCopyCachingConstants.ExpirationInMinutes);
}