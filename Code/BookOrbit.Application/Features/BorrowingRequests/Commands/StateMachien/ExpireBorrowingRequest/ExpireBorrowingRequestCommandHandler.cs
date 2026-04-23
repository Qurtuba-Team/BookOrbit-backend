namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;
public class ExpireBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<ExpireBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<ExpireBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(ExpireBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequest = await context.BorrowingRequests
            .FirstOrDefaultAsync(br => br.Id == command.BorrowingRequestId, ct);

        if (borrowingRequest is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for expiration.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var expireResult = borrowingRequest.MarkAsExpired();

        if (expireResult.IsFailure)
            return expireResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been expired.",
            borrowingRequest.Id);

        return Result.Updated;
    }
}
