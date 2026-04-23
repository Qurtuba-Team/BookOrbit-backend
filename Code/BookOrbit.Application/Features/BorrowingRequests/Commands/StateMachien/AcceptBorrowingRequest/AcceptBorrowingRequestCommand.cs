namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;

public record AcceptBorrowingRequestCommand(Guid BorrowingRequestId, Guid StudentId) : IRequest<Result<Updated>>;
