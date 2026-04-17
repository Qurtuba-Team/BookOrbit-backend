namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecordById;
public record GetLendingListRecordByIdQuery(
    Guid LendingListRecordId) : ICachedQuery<Result<LendingListRecordDto>>
{
    public string CacheKey => LendingListCachingConstants.LendingListKey(LendingListRecordId);

    public string[] Tags => [LendingListCachingConstants.LendingListTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(LendingListCachingConstants.ExpirationInMinutes);
}
