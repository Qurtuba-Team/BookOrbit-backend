namespace BookOrbit.Application.Features.Recommendations.Commands.RecordInteraction;

public record RecordInteractionCommand(
    string UserId,
    Guid BookId,
    int? Rating,
    bool IsRead,
    bool IsWishlisted) : IRequest<Result<bool>>;
