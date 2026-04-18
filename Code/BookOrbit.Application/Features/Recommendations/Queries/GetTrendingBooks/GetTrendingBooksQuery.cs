using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

namespace BookOrbit.Application.Features.Recommendations.Queries.GetTrendingBooks;

public record GetTrendingBooksQuery : IRequest<Result<List<RecommendationDto>>>;
