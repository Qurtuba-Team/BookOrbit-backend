namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.MakeAvailableLendingListRecord;
public record MakeAvailableLendingListRecordCommand(
Guid LendingListRecordId) : IRequest<Result<Updated>>;
