namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;
public class AcceptBorrowingRequestCommandHandler(
    IAppDbContext context,
    ILogger<AcceptBorrowingRequestCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<AcceptBorrowingRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AcceptBorrowingRequestCommand command, CancellationToken ct)
    {
        var borrowingRequestData = await context.BorrowingRequests
            .Select(br =>new
            {
                BorrowingRequest = br,
                OwnerId = br.LendingRecord!.BookCopy!.OwnerId //Better than doing a naviagation property in the domain model, as it doesn't require loading the related entities into memory
            })
            .FirstOrDefaultAsync(br => br.BorrowingRequest.Id == command.BorrowingRequestId, ct);

        if (borrowingRequestData is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for acceptance.",
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

        var acceptResult = borrowingRequestData.BorrowingRequest.MarkAsApproved();

        if (acceptResult.IsFailure)
            return acceptResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} has been accepted.",
            borrowingRequestData.BorrowingRequest.Id);

        return Result.Updated;
    }
}
