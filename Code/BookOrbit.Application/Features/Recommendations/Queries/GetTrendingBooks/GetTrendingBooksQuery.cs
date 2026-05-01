namespace BookOrbit.Application.Features.Recommendations.Queries.GetTrendingBooks;

public record GetTrendingBooksQuery : ICachedQuery<Result<List<RecommendationDto>>>
{
    public string CacheKey => RecommendationsCachingConstants.TrendingKey;

    public string[] Tags => [RecommendationsCachingConstants.RecommendationsTag];

    public TimeSpan Expiration =>
        TimeSpan.FromMinutes(RecommendationsCachingConstants.TrendingExpirationInMinutes);
}
