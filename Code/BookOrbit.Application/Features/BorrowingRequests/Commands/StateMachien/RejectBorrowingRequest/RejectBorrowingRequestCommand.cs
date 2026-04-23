namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;

public record RejectBorrowingRequestCommand(Guid BorrowingRequestId, Guid StudentId) : IRequest<Result<Updated>>;
