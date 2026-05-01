
namespace BookOrbit.Application.Features.LendingListings.Commands.CreateLendingListRecord;
public class CreateLendingListRecordCommandHandler(
    ILogger<CreateLendingListRecordCommandHandler> logger,
    IAppDbContext context,
    TimeProvider timeProvider,
    HybridCache cache) : IRequestHandler<CreateLendingListRecordCommand, Result<LendingListRecordDto>>
{
    public async Task<Result<LendingListRecordDto>> Handle(
        CreateLendingListRecordCommand command,
        CancellationToken ct)
    {
        var bookCopy = await context.BookCopies
            .AsNoTracking()
    .Where(bc => bc.Id == command.BookCopyId)
    .Select(bc => new
    {
        bc.OwnerId,
        bc.State
    })
    .FirstOrDefaultAsync(ct);


        if (bookCopy is null)
        {
            logger.LogWarning(
    "Book copy {BookCopyId} not found ",
    command.BookCopyId);

            return BookCopyApplicationErrors.NotFoundById;
        }

        if (bookCopy.State != BookCopyState.Available)
        {
            logger.LogWarning(
    "Book copy with id {BookCopyId} is not available for lending.",
    command.BookCopyId);

            return LendingListApplicationErrors.BookCopyIsNotAvilableForlending;
        }

        var isListed = await context.LendingListRecords.AnyAsync(llr =>
       llr.BookCopyId == command.BookCopyId
       && llr.State == LendingListRecordState.Available, ct);

        if (isListed)
        {
            logger.LogWarning(
                "Book copy with id {BookCopyId} is already listed for lending.",
                command.BookCopyId);

            return LendingListApplicationErrors.BookCopyAlreadyListed;
        }

        var now = timeProvider.GetUtcNow();

        var createdLendingListRecord = LendingListRecord.Create(
            Guid.NewGuid(),
            command.BookCopyId,
            command.BorrowingDurationInDays,
            new Point(Point.LendingRecordDefaultCost),
            now.AddDays(LendingListRecord.DefaultExpirationDurationInDays),
            now);

        if (createdLendingListRecord.IsFailure)
        {
            logger.LogWarning(
                "Failed to create lending list record for book copy with id {BookCopyId}. Errors: {Errors}",
                command.BookCopyId,
                createdLendingListRecord.Errors);
            return createdLendingListRecord.Errors;
        }

        context.LendingListRecords.Add(createdLendingListRecord.Value);
        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(LendingListCachingConstants.LendingListTag, ct);

        logger.LogInformation(
            "Lending list record with id {LendingListRecordId} created for book copy with id {BookCopyId}.",
            createdLendingListRecord.Value.Id,
            command.BookCopyId);

        return LendingListRecordDto.FromEntity(createdLendingListRecord.Value);
    }
}
