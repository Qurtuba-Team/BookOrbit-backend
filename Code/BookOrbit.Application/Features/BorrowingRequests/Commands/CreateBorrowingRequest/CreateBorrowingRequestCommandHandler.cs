
namespace BookOrbit.Application.Features.BorrowingRequests.Commands.CreateBorrowingRequest;
public class CreateBorrowingRequestCommandHandler(
    ILogger<CreateBorrowingRequestCommandHandler> logger,
    IAppDbContext context,
    TimeProvider timeProvider,
    HybridCache cache) : IRequestHandler<CreateBorrowingRequestCommand, Result<BorrowingRequestDto>>
{
    public async Task<Result<BorrowingRequestDto>> Handle(CreateBorrowingRequestCommand command, CancellationToken ct)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == command.BorrowingStudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Borrowing student not found. StudentId: {StudentId}", command.BorrowingStudentId);
            return StudentApplicationErrors.NotFoundById;
        }

        var lendingRecord = await context.LendingListRecords
            .AsNoTracking()
            .Where(lr => lr.Id == command.LendingRecordId)
            .Select(lr => new { lr.State, lr.BookCopy!.OwnerId , Cost = lr.Cost.Value})
            .FirstOrDefaultAsync(ct);

        if (lendingRecord is null)
        {
            logger.LogWarning("Lending list record not found. LendingRecordId: {LendingRecordId}", command.LendingRecordId);
            return LendingListApplicationErrors.NotFoundById;
        }

        if (lendingRecord.State is not LendingListRecordState.Available)
        {
            logger.LogWarning("Lending list record {LendingRecordId} is not available.", command.LendingRecordId);
            return BorrowingRequestApplicationErrors.LendingRecordNotAvailable;
        }

        if (lendingRecord.OwnerId == command.BorrowingStudentId)
        {
            logger.LogWarning("Student {StudentId} cannot request to borrow their own book copy.", command.BorrowingStudentId);
            return BorrowingRequestApplicationErrors.StudentCannotBorrowOwnedCopies;
        }
            var existingRequest = await context.BorrowingRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(br =>
                    br.BorrowingStudentId == command.BorrowingStudentId &&
                    (br.LendingRecordId == command.LendingRecordId 
                    && (
                    br.State == BorrowingRequestState.Pending ||
                    br.State == BorrowingRequestState.Accepted))
                
                    , ct);

        if (existingRequest is not null)
        {
            logger.LogWarning("Borrowing request already exists for student {StudentId} and lending record {LendingRecordId}.", command.BorrowingStudentId, command.LendingRecordId);
            return BorrowingRequestApplicationErrors.AlreadyExists;
        }

        var now = timeProvider.GetUtcNow();

        var borrowingRequestResult = BorrowingRequest.Create(
            Guid.NewGuid(),
            command.BorrowingStudentId,
            command.LendingRecordId,
            now.AddDays(BorrowingRequest.DefaultExpirationDays),
            now);

        if (borrowingRequestResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create borrowing request for student {StudentId}. Errors: {Errors}",
                command.BorrowingStudentId,
                borrowingRequestResult.Errors);
            return borrowingRequestResult.Errors;
        }

        //Take The Points (temp)
        var pointsToDeductResult = Point.Create(lendingRecord.Cost);

        if(pointsToDeductResult.IsFailure)
        {
            logger.LogWarning(
                "Invalid points to deduct for student {StudentId}. Errors: {Errors}",
                command.BorrowingStudentId,
                pointsToDeductResult.Errors);
            return pointsToDeductResult.Errors;
        }

        var deductingPointsResult = student.DeductPoints(pointsToDeductResult.Value, PointTransactionReason.Borrowing);
        if (deductingPointsResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to deduct points for student {StudentId}. Errors: {Errors}",
                command.BorrowingStudentId,
                deductingPointsResult.Errors);
            return deductingPointsResult.Errors;
        }   

        context.BorrowingRequests.Add(borrowingRequestResult.Value);
        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingRequestCachingConstants.BorrowingRequestTag, ct);

        logger.LogInformation(
            "Borrowing request {BorrowingRequestId} created for student {StudentId}.",
            borrowingRequestResult.Value.Id,
            command.BorrowingStudentId);

        return BorrowingRequestDto.FromEntity(borrowingRequestResult.Value);
    }
}
