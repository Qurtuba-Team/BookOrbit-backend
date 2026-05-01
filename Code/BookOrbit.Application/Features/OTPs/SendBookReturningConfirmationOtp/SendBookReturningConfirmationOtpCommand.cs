namespace BookOrbit.Application.Features.OTPs.SendBookReturningConfirmationOtp;

public record SendBookReturningConfirmationOtpCommand(
    Guid BorrowingTransactionId) : IRequest<Result<Success>>;