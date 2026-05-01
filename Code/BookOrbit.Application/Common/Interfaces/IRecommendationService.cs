using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

namespace BookOrbit.Application.Common.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Builds personalised recommendations for the given user.
    /// Delegates to Infrastructure because it requires <c>UserManager&lt;AppUser&gt;</c>.
    /// </summary>
    Task<Result<List<RecommendationDto>>> GetRecommendationsAsync(string userId, CancellationToken ct);
}
