namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;

public record ExpireBorrowingRequestCommand(Guid BorrowingRequestId) : IRequest<Result<Updated>>;
