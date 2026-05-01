namespace BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

public class GetRecommendationsQueryHandler(
    IRecommendationService recommendationService,
    ILogger<GetRecommendationsQueryHandler> logger)
    : IRequestHandler<GetRecommendationsQuery, Result<List<RecommendationDto>>>
{
    public async Task<Result<List<RecommendationDto>>> Handle(
        GetRecommendationsQuery query,
        CancellationToken ct)
    {
        logger.LogDebug("Fetching personalised recommendations for UserId={UserId}.", query.UserId);

        return await recommendationService.GetRecommendationsAsync(query.UserId, ct);
    }
}
