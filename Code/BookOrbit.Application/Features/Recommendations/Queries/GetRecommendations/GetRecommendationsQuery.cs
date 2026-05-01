namespace BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

public record GetRecommendationsQuery(string UserId)
    : ICachedQuery<Result<List<RecommendationDto>>>
{
    public string CacheKey => RecommendationsCachingConstants.UserRecommendationsKey(UserId);

    public string[] Tags => [RecommendationsCachingConstants.RecommendationsTag];

    public TimeSpan Expiration =>
        TimeSpan.FromMinutes(RecommendationsCachingConstants.RecommendationsExpirationInMinutes);
}
