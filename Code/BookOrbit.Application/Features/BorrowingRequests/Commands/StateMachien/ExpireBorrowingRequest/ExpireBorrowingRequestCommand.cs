namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;

public record ExpireBorrowingRequestCommand(Guid BorrowingRequestId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
