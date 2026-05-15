namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;

public record CancelBorrowingRequestCommand(Guid BorrowingRequestId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
