namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecordById;
public class GetLendingListRecordByIdQueryHandler(
    ILogger<GetLendingListRecordByIdQueryHandler> logger,
    IAppDbContext context) : IRequestHandler<GetLendingListRecordByIdQuery, Result<LendingListRecordDto>>
{
    public async Task<Result<LendingListRecordDto>> Handle(GetLendingListRecordByIdQuery query, CancellationToken ct)
    {
        var lendingListRecord = await context.LendingListRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(lr => lr.Id == query.LendingListRecordId, ct);

        if (lendingListRecord is null)
        {
            logger.LogWarning("Lending list record {LendingListRecordId} not found.", query.LendingListRecordId);

            return LendingListApplicationErrors.NotFoundById;
        }

        return LendingListRecordDto.FromEntity(lendingListRecord);
    }
}
