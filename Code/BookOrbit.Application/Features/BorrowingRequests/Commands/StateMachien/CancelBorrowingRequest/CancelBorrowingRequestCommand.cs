namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;

public record CancelBorrowingRequestCommand(Guid BorrowingRequestId) : IRequest<Result<Updated>>;
