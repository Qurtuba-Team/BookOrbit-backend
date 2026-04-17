namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.CloseLendingListRecord;
public class CloseLendingListRecordCommandHandler(
    IAppDbContext context,
    ILogger<CloseLendingListRecordCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<CloseLendingListRecordCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(CloseLendingListRecordCommand command, CancellationToken ct)
    {
        var lendingListRecord = await context.LendingListRecords
            .FirstOrDefaultAsync(llr => llr.Id == command.LendingListRecordId, ct);

        if (lendingListRecord is null)
        {
            logger.LogWarning(
                "Lending list record {LendingListRecordId} not found for closing.",
                command.LendingListRecordId);

            return LendingListApplicationErrors.NotFoundById;
        }

        var closeResult = lendingListRecord.MarkAsClosed();

        if (closeResult.IsFailure)
            return closeResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(LendingListCachingConstants.LendingListTag, ct);


        logger.LogInformation(
            "Lending list record {LendingListRecordId} has been closed",
            lendingListRecord.Id);

        return Result.Updated;
    }
}
