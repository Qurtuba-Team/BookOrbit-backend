using BookOrbit.Application.Features.BorrowingReviews.Dtos;

namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviews;

public record GetBorrowingReviewsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? ReviewerStudentId = null,
    Guid? ReviewedStudentId = null,
    Guid? BorrowingTransactionId = null)
    : ICachedQuery<Result<PaginatedList<BorrowingReviewListItemDto>>>
{
    public string CacheKey => BorrowingReviewCachingConstants.BorrowingReviewListKey(this);

    public string[] Tags => [BorrowingReviewCachingConstants.BorrowingReviewTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingReviewCachingConstants.ExpirationInMinutes);
}
