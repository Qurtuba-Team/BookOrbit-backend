using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;
public class CreateBorrowingTransactionCommandHandler(
    ILogger<CreateBorrowingTransactionCommandHandler> logger,
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<CreateBorrowingTransactionCommand, Result<BorrowingTransactionDto>>
{
    public async Task<Result<BorrowingTransactionDto>> Handle(CreateBorrowingTransactionCommand command, CancellationToken ct)
    {
        var borrowingRequest = await context.BorrowingRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(br => br.Id == command.BorrowingRequestId, ct);

        if (borrowingRequest is null)
        {
            logger.LogWarning("Borrowing request {BorrowingRequestId} not found.", command.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        if(borrowingRequest.State is not BorrowingRequestState.Accepted)
        {
            logger.LogWarning("Borrowing request {BorrowingRequestId} is not in accepted state.", command.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.BorrowingRequestNotAccepted;
        }

        var lendingRecord = await context.LendingListRecords
            .AsNoTracking()
            .Where(lr => lr.Id == borrowingRequest.LendingRecordId)
            .Select(lr => new
            {
                lr.BookCopyId,
                lr.BorrowingDurationInDays,
                lr.BookCopy!.OwnerId
            })
            .FirstOrDefaultAsync(ct);


        if (lendingRecord is null)
        {
            logger.LogWarning("Lending list record {LendingRecordId} not found.", borrowingRequest.LendingRecordId);
            return LendingListApplicationErrors.NotFoundById;
        }

        var now = timeProvider.GetUtcNow();
        var expectedReturnDate = now.AddDays(lendingRecord.BorrowingDurationInDays);

        var transactionResult = BorrowingTransaction.Create(
            Guid.NewGuid(),
            borrowingRequest.Id,
            lendingRecord.OwnerId,
            borrowingRequest.BorrowingStudentId,
            lendingRecord.BookCopyId,
            expectedReturnDate,
            now);

        if (transactionResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create borrowing transaction for request {BorrowingRequestId}. Errors: {Errors}",
                borrowingRequest.Id,
                transactionResult.Errors);
            return transactionResult.Errors;
        }

        context.BorrowingTransactions.Add(transactionResult.Value);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} created for borrowing request {BorrowingRequestId}.",
            transactionResult.Value.Id,
            borrowingRequest.Id);

        return BorrowingTransactionDto.FromEntity(transactionResult.Value);
    }
}
