using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

namespace BookOrbit.Application.Features.Recommendations.Queries.GetTrendingBooks;

public class GetTrendingBooksQueryHandler(
    IAppDbContext context,
    ILogger<GetTrendingBooksQueryHandler> logger)
    : IRequestHandler<GetTrendingBooksQuery, Result<List<RecommendationDto>>>
{
    private const int TopN = 20;

    public async Task<Result<List<RecommendationDto>>> Handle(
        GetTrendingBooksQuery query,
        CancellationToken ct)
    {
        logger.LogDebug("Building trending books list (top {Count}).", TopN);

        var trending = await context.Books
            .Select(b => new
            {
                Book        = b,
                AvgRating   = context.UserBookInteractions
                                  .Where(i => i.BookId == b.Id && i.Rating.HasValue)
                                  .Select(i => (double?)i.Rating)
                                  .Average() ?? 0.0,
                RatingsCount = context.UserBookInteractions
                                  .Count(i => i.BookId == b.Id && i.Rating.HasValue)
            })
            .OrderByDescending(x => x.AvgRating)
            .ThenByDescending(x => x.RatingsCount)
            .Take(TopN)
            .ToListAsync(ct);

        var result = trending
            .Select(x => new RecommendationDto(
                BookId:      x.Book.Id,
                Title:       x.Book.Title.Value,
                Author:      x.Book.Author.Value,
                Genre:       x.Book.Category.ToString(),
                Level:       string.Empty,
                ReasonLabel: "Trending",
                Score:       x.AvgRating))
            .ToList();

        logger.LogDebug("Trending books list built with {Count} entries.", result.Count);

        return result;
    }
}
