namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.MakeAvailableLendingListRecord;
public class MakeAvailableLendingListRecordCommandHandler(
    IAppDbContext context,
    ILogger<MakeAvailableLendingListRecordCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<MakeAvailableLendingListRecordCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MakeAvailableLendingListRecordCommand command, CancellationToken ct)
    {
        var lendingListRecord = await context.LendingListRecords
            .FirstOrDefaultAsync(llr => llr.Id == command.LendingListRecordId, ct);

        if (lendingListRecord is null)
        {
            logger.LogWarning(
                "Lending list record {LendingListRecordId} not found for making available.",
                command.LendingListRecordId);

            return LendingListApplicationErrors.NotFoundById;
        }

        var availableResult = lendingListRecord.MarkAsAvailable();

        if (availableResult.IsFailure)
            return availableResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(LendingListCachingConstants.LendingListTag, ct);

        logger.LogInformation(
            "Lending list record {LendingListRecordId} has been marked as available",
            lendingListRecord.Id);

        return Result.Updated;
    }
}
