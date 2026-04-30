namespace BookOrbit.Application.Common.OTPs.VerifyBookDeliveryConfirmationOtp;
public record VerifyBookDeliveryConfirmationOtpCommand(
    Guid BorrowingRequestId,
    string Otp) : IRequest<Result<Success>>;