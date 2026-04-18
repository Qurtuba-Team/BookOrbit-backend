namespace BookOrbit.Api.Contracts.Requests.Recommendations;

public record RecordInteractionRequest(
    Guid BookId,
    int? Rating,
    bool IsRead,
    bool IsWishlisted);
