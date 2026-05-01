
namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;
public class CancelBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<CancelBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<CancelBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(CancelBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequestData = await context.BorrowingRequests
            .Select(br => new
            {
                BorrowingRequest = br,
                BorrowingStudent = br.BorrowingStudent,
                Cost = br.LendingRecord!.Cost.Value,
                LendingRecord = br.LendingRecord
            })
            .FirstOrDefaultAsync(br => br.BorrowingRequest.Id == command.BorrowingRequestId, ct);

        if (borrowingRequestData is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for cancellation.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var cancelResult = borrowingRequestData.BorrowingRequest.MarkAsCancelled();

        if (cancelResult.IsFailure)
            return cancelResult.Errors;


        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been cancelled.",
            borrowingRequestData.BorrowingRequest.Id);

        return Result.Updated;
    }
}
