namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequestById;
public record GetBorrowingRequestByIdQuery(
    Guid BorrowingRequestId) : ICachedQuery<Result<BorrowingRequestDto>>
{
    public string CacheKey => BorrowingRequestCachingConstants.BorrowingRequestKey(BorrowingRequestId);

    public string[] Tags => [BorrowingRequestCachingConstants.BorrowingRequestTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingRequestCachingConstants.ExpirationInMinutes);
}
