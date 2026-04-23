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
                 OwnerId = br.LendingRecord!.BookCopy!.OwnerId //Better than doing a naviagation property in the domain model, as it doesn't require loading the related entities into memory
             })
             .FirstOrDefaultAsync(br => br.BorrowingRequest.Id == command.BorrowingRequestId, ct);

        if (borrowingRequestData is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for rejection.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var ownerId = borrowingRequestData.OwnerId;

        if (ownerId != command.StudentId)
        {
            logger.LogWarning(
                "Student {StudentId} is not the owner of borrowing request {BorrowingRequestId}.",
                command.StudentId,
                borrowingRequestData.BorrowingRequest.Id);

            return BorrowingRequestApplicationErrors.StudentNotLendingRecordOwner;
        }

        var rejectResult = borrowingRequestData.BorrowingRequest.MarkAsRejected();

        if (rejectResult.IsFailure)
            return rejectResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been rejected.",
            borrowingRequestData.BorrowingRequest.Id);

        return Result.Updated;
    }
}
