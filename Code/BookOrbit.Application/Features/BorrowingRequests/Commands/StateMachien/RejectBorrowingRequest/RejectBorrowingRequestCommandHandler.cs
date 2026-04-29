
namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;
public class RejectBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<RejectBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RejectBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RejectBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequestData = await context.BorrowingRequests
            .Select(br => new
            {
                BorrowingRequest = br,
                BorrowingStudent = br.BorrowingStudent,
                Cost = br.LendingRecord!.Cost.Value
            })
            .FirstOrDefaultAsync(br => br.BorrowingRequest.Id == command.BorrowingRequestId, ct);

        if (borrowingRequestData is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for rejection.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var rejectResult = borrowingRequestData.BorrowingRequest.MarkAsRejected();

        if (rejectResult.IsFailure)
            return rejectResult.Errors;

        //retrive the points to the student that has been deducted when the borrowing request was created
        var addingPointResult = borrowingRequestData.BorrowingStudent!.AddPoints(Point.Create(borrowingRequestData.Cost).Value);

        if (addingPointResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to add points for student {StudentId}. Errors: {Errors}",
                borrowingRequestData.BorrowingStudent.Id,
                addingPointResult.Errors);
            return addingPointResult.Errors;
        }

        var pointTransactionResult = PointTransaction.Create(
            Guid.NewGuid(),
            borrowingRequestData.BorrowingStudent.Id,
            null,
            borrowingRequestData.Cost,
            PointTransactionReason.Refund);

        if(pointTransactionResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create point transaction for student {StudentId}. Errors: {Errors}",
                borrowingRequestData.BorrowingStudent.Id,
                pointTransactionResult.Errors);
            return pointTransactionResult.Errors;
        }

        context.PointTransactions.Add(pointTransactionResult.Value);

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been rejected.",
            borrowingRequestData.BorrowingRequest.Id);

        return Result.Updated;
    }
}
