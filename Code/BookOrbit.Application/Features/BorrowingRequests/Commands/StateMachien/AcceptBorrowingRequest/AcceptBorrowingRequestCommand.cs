namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;

public record AcceptBorrowingRequestCommand(Guid BorrowingRequestId) : IRequest<Result<Updated>>;
