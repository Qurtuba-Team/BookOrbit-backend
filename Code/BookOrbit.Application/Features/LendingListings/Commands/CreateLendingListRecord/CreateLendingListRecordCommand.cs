namespace BookOrbit.Application.Features.LendingListings.Commands.CreateLendingListRecord;

public record CreateLendingListRecordCommand(
        Guid BookCopyId,
        int BorrowingDurationInDays) : IRequest<Result<LendingListRecordDto>>;