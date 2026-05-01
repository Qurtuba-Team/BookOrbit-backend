namespace BookOrbit.Application.Features.OTPs.VerifyBookReturningConfirmationOtp;

public record VerifyBookReturningConfirmationOtpCommand(
    Guid BorrowingTransactionId,
    string Otp) : IRequest<Result<Success>>;