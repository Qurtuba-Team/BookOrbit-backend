namespace BookOrbit.Application.Features.LendingListings.Commands.CreateLendingListRecord;

public record CreateLendingListRecordCommand(
        Guid BookCopyId,
        Guid OwnerId,
        int BorrowingDurationInDays) : IRequest<Result<LendingListRecordDto>>;