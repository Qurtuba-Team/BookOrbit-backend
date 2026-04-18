using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

namespace BookOrbit.Application.Common.Interfaces;

public interface IRecommendationService
{
    Task<Result<List<RecommendationDto>>> GetRecommendationsAsync(string userId, CancellationToken ct);
    Task<Result<List<RecommendationDto>>> GetTrendingBooksAsync(CancellationToken ct);
}
