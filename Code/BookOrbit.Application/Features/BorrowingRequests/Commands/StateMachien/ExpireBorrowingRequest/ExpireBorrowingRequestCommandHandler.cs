
namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;
public class ExpireBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<ExpireBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<ExpireBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(ExpireBorrowingRequestCommand command, CancellationToken ct)
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
                "Borrowing request {BorrowingRequestId} not found for expiration.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var expireResult = borrowingRequestData.BorrowingRequest.MarkAsExpired();

        if (expireResult.IsFailure)
            return expireResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been expired.",
            borrowingRequestData.BorrowingRequest.Id);

        return Result.Updated;
    }
}
