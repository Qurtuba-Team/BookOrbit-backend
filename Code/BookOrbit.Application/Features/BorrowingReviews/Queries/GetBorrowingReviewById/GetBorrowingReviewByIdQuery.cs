
namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviewById;

public record GetBorrowingReviewByIdQuery(Guid BorrowingReviewId)
    : ICachedQuery<Result<BorrowingReviewDto>>
{
    public string CacheKey => BorrowingReviewCachingConstants.BorrowingReviewKey(BorrowingReviewId);

    public string[] Tags => [BorrowingReviewCachingConstants.BorrowingReviewTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingReviewCachingConstants.ExpirationInMinutes);
}
