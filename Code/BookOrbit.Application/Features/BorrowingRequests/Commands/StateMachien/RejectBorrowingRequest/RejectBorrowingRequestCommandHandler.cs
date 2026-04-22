namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;
public class RejectBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<RejectBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RejectBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RejectBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequest = await context.BorrowingRequests
            .FirstOrDefaultAsync(br => br.Id == command.BorrowingRequestId, ct);

        if (borrowingRequest is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for rejection.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var rejectResult = borrowingRequest.MarkAsRejected();

        if (rejectResult.IsFailure)
            return rejectResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been rejected.",
            borrowingRequest.Id);

        return Result.Updated;
    }
}
