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
                StudentPoints = br.BorrowingStudent!.Points,
                 br.LendingRecord!.Cost,
                br.LendingRecord!.BookCopy!.OwnerId //Better than doing a naviagation property in the domain model, as it doesn't require loading the related entities into memory
            })
            .FirstOrDefaultAsync(br => br.BorrowingRequest.Id == command.BorrowingRequestId, ct);

        if (borrowingRequestData is null)
        {
            logger.LogWarning(
                "Borrowing request {BorrowingRequestId} not found for acceptance.",
                command.BorrowingRequestId);

            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var isAlreadyHasAceptedRequest = await context.BorrowingRequests
            .AnyAsync(br =>
                br.LendingRecordId == borrowingRequestData.BorrowingRequest.LendingRecordId &&
                br.State == BorrowingRequestState.Accepted, ct);

        if(isAlreadyHasAceptedRequest) //Cannot Accept a borrowing request if there is already an accepted request for the same lending record, as the lending record is not available anymore
        {
            logger.LogWarning(
                "Lending record {LendingRecordId} already has an accepted borrowing request.",
                borrowingRequestData.BorrowingRequest.LendingRecordId);
            return BorrowingRequestApplicationErrors.LendingRecordNotAvailable;
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
