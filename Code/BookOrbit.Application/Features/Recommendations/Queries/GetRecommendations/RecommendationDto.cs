namespace BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

public record RecommendationDto(
    Guid BookId,
    string Title,
    string Author,
    string Genre,
    string Level,
    string ReasonLabel,
    double Score);
