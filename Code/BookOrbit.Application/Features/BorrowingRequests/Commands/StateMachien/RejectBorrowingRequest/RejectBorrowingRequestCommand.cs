namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;

public record RejectBorrowingRequestCommand(Guid BorrowingRequestId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
