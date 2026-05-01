namespace BookOrbit.Application.Features.OTPs.VerifyBookDeliveryConfirmationOtp;
public record VerifyBookDeliveryConfirmationOtpCommand(
    Guid BorrowingRequestId,
    string Otp) : IRequest<Result<Success>>;