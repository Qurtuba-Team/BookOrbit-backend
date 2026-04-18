namespace BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

public class GetRecommendationsQueryHandler(
    IRecommendationService recommendationService) : IRequestHandler<GetRecommendationsQuery, Result<List<RecommendationDto>>>
{
    public async Task<Result<List<RecommendationDto>>> Handle(GetRecommendationsQuery query, CancellationToken ct)
    {
        return await recommendationService.GetRecommendationsAsync(query.UserId, ct);
    }
}
