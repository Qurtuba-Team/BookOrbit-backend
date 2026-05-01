namespace BookOrbit.Application.Features.OTPs.VerifyBookDeliveryConfirmationOtp;
public class VerifyBookDeliveryConfirmationOtpCommandHandler(
    ILogger<VerifyBookDeliveryConfirmationOtpCommandHandler> logger,
    IBorrowingRequestOtpService otpService,
    TimeProvider timeProvider) : IRequestHandler<VerifyBookDeliveryConfirmationOtpCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(VerifyBookDeliveryConfirmationOtpCommand command, CancellationToken ct)
    {
        var isValidOtpResult = await otpService.VerifyOtp(
            command.Otp,
            command.BorrowingRequestId,
            timeProvider.GetUtcNow(),
            ct);

        if(isValidOtpResult.IsFailure)
        {
            logger.LogWarning("Failed to verify OTP for borrowing request with id {BorrowingRequestId}. Errors: {Errors}",
                command.BorrowingRequestId, isValidOtpResult.Errors);
            return isValidOtpResult.Errors;
        }
        
        logger.LogInformation("Successfully verified OTP for borrowing request with id {BorrowingRequestId}.", command.BorrowingRequestId);
        return Result.Success;
    }
}