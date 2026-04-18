namespace BookOrbit.Application.Features.BorrowingRequests.Commands.CreateBorrowingRequest;

public record CreateBorrowingRequestCommand(
    Guid BorrowingStudentId,
    Guid LendingRecordId) : IRequest<Result<BorrowingRequestDto>>;
