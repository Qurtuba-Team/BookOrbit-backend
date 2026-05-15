namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;

public record AcceptBorrowingRequestCommand(Guid BorrowingRequestId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
