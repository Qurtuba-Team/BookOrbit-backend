namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;
public class AcceptBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<AcceptBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<AcceptBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AcceptBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequest = await context.BorrowingRequests
            .FirstOrDefaultAsync(br => br.Id == command.BorrowingRequestId, ct);

        if (borrowingRequest is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for acceptance.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var acceptResult = borrowingRequest.MarkAsApproved();

        if (acceptResult.IsFailure)
            return acceptResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been accepted.",
            borrowingRequest.Id);

        return Result.Updated;
    }
}
