
namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviews;

public record GetBorrowingReviewsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    Guid? ReviewerStudentId = null,
    Guid? ReviewedStudentId = null,
    Guid? BorrowingTransactionId = null)
    : IPagedQuery<BorrowingReviewListItemDto>
{
    public string CacheKey => BorrowingReviewCachingConstants.BorrowingReviewListKey(this);

    public string[] Tags => [BorrowingReviewCachingConstants.BorrowingReviewTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingReviewCachingConstants.ExpirationInMinutes);
}
