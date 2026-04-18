namespace BookOrbit.Application.Features.Recommendations.Commands.RecordInteraction;

public class RecordInteractionCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ILogger<RecordInteractionCommandHandler> logger) : IRequestHandler<RecordInteractionCommand, Result<bool>>
{
    private const string RecommendationsCacheKeyPrefix = "recommendations:";

    public async Task<Result<bool>> Handle(RecordInteractionCommand command, CancellationToken ct)
    {
        var existing = await context.UserBookInteractions
            .FirstOrDefaultAsync(x => x.UserId == command.UserId && x.BookId == command.BookId, ct);

        if (existing is null)
        {
            var interaction = new UserBookInteraction
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                BookId = command.BookId,
                Rating = command.Rating,
                IsRead = command.IsRead,
                IsWishlisted = command.IsWishlisted,
                InteractionDate = DateTime.UtcNow
            };

            context.UserBookInteractions.Add(interaction);
        }
        else
        {
            existing.Rating = command.Rating;
            existing.IsRead = command.IsRead;
            existing.IsWishlisted = command.IsWishlisted;
            existing.InteractionDate = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);

        var cacheKey = $"{RecommendationsCacheKeyPrefix}{command.UserId}";
        await cache.RemoveAsync(cacheKey, ct);

        logger.LogInformation(
            "Recorded interaction for UserId={UserId}, BookId={BookId}. Cache invalidated.",
            command.UserId,
            command.BookId);

        return true;
    }
}
