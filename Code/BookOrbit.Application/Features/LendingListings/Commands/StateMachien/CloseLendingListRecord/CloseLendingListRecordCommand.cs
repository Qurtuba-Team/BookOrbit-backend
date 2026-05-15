namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.CloseLendingListRecord;
public record CloseLendingListRecordCommand(
    Guid LendingListRecordId,
    string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
