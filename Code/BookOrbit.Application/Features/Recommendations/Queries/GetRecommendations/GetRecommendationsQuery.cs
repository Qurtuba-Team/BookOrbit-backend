namespace BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

public record GetRecommendationsQuery(string UserId) : IRequest<Result<List<RecommendationDto>>>;
