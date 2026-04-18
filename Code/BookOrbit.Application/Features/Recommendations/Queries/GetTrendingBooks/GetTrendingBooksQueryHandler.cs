using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

namespace BookOrbit.Application.Features.Recommendations.Queries.GetTrendingBooks;

public class GetTrendingBooksQueryHandler(
    IRecommendationService recommendationService) : IRequestHandler<GetTrendingBooksQuery, Result<List<RecommendationDto>>>
{
    public async Task<Result<List<RecommendationDto>>> Handle(GetTrendingBooksQuery query, CancellationToken ct)
    {
        return await recommendationService.GetTrendingBooksAsync(ct);
    }
}
