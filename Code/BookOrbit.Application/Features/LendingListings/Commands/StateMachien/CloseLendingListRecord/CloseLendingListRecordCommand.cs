namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.CloseLendingListRecord;
public record CloseLendingListRecordCommand(
Guid LendingListRecordId) : IRequest<Result<Updated>>;
