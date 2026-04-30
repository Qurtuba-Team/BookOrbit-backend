namespace BookOrbit.Application.Common.OTPs.SendOtp;
public record SendBookDeliveryConfirmationOtpCommand(
    Guid BorrowingRequestId) : IRequest<Result<Success>>;