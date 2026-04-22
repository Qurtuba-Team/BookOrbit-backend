namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;
public class CancelBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<CancelBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<CancelBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(CancelBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequest = await context.BorrowingRequests
            .FirstOrDefaultAsync(br => br.Id == command.BorrowingRequestId, ct);

        if (borrowingRequest is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for cancellation.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var cancelResult = borrowingRequest.MarkAsCancelled();

        if (cancelResult.IsFailure)
            return cancelResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been cancelled.",
            borrowingRequest.Id);

        return Result.Updated;
    }
}
