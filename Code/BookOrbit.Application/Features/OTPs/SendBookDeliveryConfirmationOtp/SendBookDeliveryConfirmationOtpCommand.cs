namespace BookOrbit.Application.Features.OTPs.SendBookDeliveryConfirmationOtp;
public record SendBookDeliveryConfirmationOtpCommand(
    Guid BorrowingRequestId) : IRequest<Result<Success>>;