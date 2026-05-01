namespace BookOrbit.Application.Common.Constants;

public static class RecommendationsCachingConstants
{
    public const string RecommendationsTag = "recommendations";

    public static string UserRecommendationsKey(string userId)
        => $"recommendations:{userId}";

    public const string TrendingKey = "recommendations:trending";

    public const int RecommendationsExpirationInMinutes = 360;   // 6 hours
    public const int TrendingExpirationInMinutes = 1440;         // 24 hours
}
