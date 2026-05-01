
namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;
public class CreateBorrowingTransactionCommandHandler(
    ILogger<CreateBorrowingTransactionCommandHandler> logger,
    IAppDbContext context,
    TimeProvider timeProvider,
    HybridCache hybridCache)
    : IRequestHandler<CreateBorrowingTransactionCommand, Result<BorrowingTransactionDto>>
{
    public async Task<Result<BorrowingTransactionDto>> Handle(CreateBorrowingTransactionCommand command, CancellationToken ct)
    {
        var borrowingRequestData = await context.BorrowingRequests
            .Select(br =>
            new {
                borrowingRequest = br,
                bookCopy = br!.LendingRecord!.BookCopy,
                lendingRecord = br.LendingRecord
            })
            .FirstOrDefaultAsync(br => br.borrowingRequest.Id== command.BorrowingRequestId, ct);

        if (borrowingRequestData is null)
        {
            logger.LogWarning("Borrowing request {BorrowingRequestId} not found.", command.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        if(borrowingRequestData.borrowingRequest.State is not BorrowingRequestState.Accepted)
        {
            logger.LogWarning("Borrowing request {BorrowingRequestId} is not in accepted state.", command.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.BorrowingRequestNotAccepted;
        }

        var lendingRecord = await context.LendingListRecords
            .AsNoTracking()
            .Where(lr => lr.Id == borrowingRequestData.borrowingRequest.LendingRecordId)
            .Select(lr => new
            {
                lr.BookCopyId,
                lr.BorrowingDurationInDays,
                lr.BookCopy!.OwnerId
            })
            .FirstOrDefaultAsync(ct);


        if (lendingRecord is null)
        {
            logger.LogWarning("Lending list record {LendingRecordId} not found.", borrowingRequestData.borrowingRequest.LendingRecordId);
            return LendingListApplicationErrors.NotFoundById;
        }

        var now = timeProvider.GetUtcNow();
        var expectedReturnDate = now.AddDays(lendingRecord.BorrowingDurationInDays);

        var transactionResult = BorrowingTransaction.Create(
            Guid.NewGuid(),
            borrowingRequestData.borrowingRequest.Id,
            lendingRecord.OwnerId,
            borrowingRequestData.borrowingRequest.BorrowingStudentId,
            lendingRecord.BookCopyId,
            expectedReturnDate,
            now);

        if (transactionResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create borrowing transaction for request {BorrowingRequestId}. Errors: {Errors}",
                borrowingRequestData.borrowingRequest.Id,
                transactionResult.Errors);
            return transactionResult.Errors;
        }

        var bookCopyBorrowingResult = borrowingRequestData!.bookCopy!.MarkAsBorrowed();

        if (bookCopyBorrowingResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to mark book copy {BookCopyId} as borrowed for borrowing request {BorrowingRequestId}. Errors: {Errors}",
                lendingRecord.BookCopyId,
                borrowingRequestData.borrowingRequest.Id,
                bookCopyBorrowingResult.Errors);
            return bookCopyBorrowingResult.Errors;
        }

        var lendingRecordBorrowingResult = borrowingRequestData.lendingRecord!.MarkAsBorrowed();

        if (lendingRecordBorrowingResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to mark lending record {LendingRecordId} as borrowed for borrowing request {BorrowingRequestId}. Errors: {Errors}",
                borrowingRequestData.lendingRecord.Id,
                borrowingRequestData.borrowingRequest.Id,
                lendingRecordBorrowingResult.Errors);
            return lendingRecordBorrowingResult.Errors;
        }

        var logCreationResult = BorrowingTransactionEvent.Create(
            Guid.NewGuid(),
            transactionResult.Value.Id,
            transactionResult.Value.State);

        if(logCreationResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create borrowing transaction event for transaction {BorrowingTransactionId}. Errors: {Errors}",
                transactionResult.Value.Id,
                logCreationResult.Errors);
            return logCreationResult.Errors;
        }

        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == lendingRecord.OwnerId, cancellationToken: ct);

        if(student is null)
        {
            logger.LogWarning(
                "Student {StudentId} not found for lending record {LendingRecordId}.",
                lendingRecord.OwnerId,
                borrowingRequestData.lendingRecord.Id);
            return StudentApplicationErrors.NotFoundById;
        }

        var pointToAddCreationResult = Point.Create(Point.DeliveringBookReward);

        if(pointToAddCreationResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create points for student {StudentId}. Errors: {Errors}",
                student.Id,
                pointToAddCreationResult.Errors);
            return pointToAddCreationResult.Errors;
        }

        var pointAdditionResult = student!.AddPoints(pointToAddCreationResult.Value);

        var pointLogCreationResult = PointTransaction.Create(
            Guid.NewGuid(),
            student.Id,
            null,
            pointToAddCreationResult.Value.Value,
            PointTransactionReason.BookBorrowedFrom);

        if(pointLogCreationResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create point transaction for student {StudentId} for borrowing transaction {BorrowingTransactionId}. Errors: {Errors}",
                student.Id,
                transactionResult.Value.Id,
                pointLogCreationResult.Errors);
            return pointLogCreationResult.Errors;
        }

        context.PointTransactions.Add(pointLogCreationResult.Value);
        context.BorrowingTransactionEvents.Add(logCreationResult.Value);
        context.BorrowingTransactions.Add(transactionResult.Value);

        await context.SaveChangesAsync(ct);
        await hybridCache.RemoveByTagAsync(BorrowingTransactionCachingConstants.BorrowingTransactionTag, ct);


        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} created for borrowing request {BorrowingRequestId}.",
            transactionResult.Value.Id,
            borrowingRequestData.borrowingRequest.Id);

        return BorrowingTransactionDto.FromEntity(transactionResult.Value);
    }
}
